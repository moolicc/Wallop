using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using WallApp.Bridge;
using WallApp.Engine.Scripting;
using WallApp.Engine.Services;
using Color = Microsoft.Xna.Framework.Color;
using ServiceProvider = WallApp.Engine.Services.ServiceProvider;
using SystemInformation = System.Windows.Forms.SystemInformation;



/*
 * General refactoring
 *
 * Redo the service locator entirely.
 *
 * Refactor the ServiceProvider. Maybe just remove the automatic reference resolution entirely to remove the compiler warnings
 * to aid in consistency and reduction in unnecessary complexity. Also refactor all the existing services.
 *
 * When a user attempts to resize a layer in EditMode, the engine just needs to display a preview box to show where the
 * layer will end up. This will prevent the associated controller's rendertarget from being re-created with every
 * single mouse-movement. Additionally, when the user adjusts the layer's size, the engine needs to communicate that
 * back to the app. ** WHY? **
 *
 * The engine needs to respond to the layerdeletion message from the app.
 */



namespace WallApp.Engine
{
    class Game : Microsoft.Xna.Framework.Game
    {
        private const double RADIAN_MULTIPLIER = 0.0174532925199433D;


        //TODO: Drag + Drop layer editing
        //TODO: Logging
        //TODO: Error/InvalidInput handling
        //TODO: Refactoring
        //TODO: LayerEditorWindow needs layout boxes to be numeric boxes with capped input./
        //TODO: Error reporting available to layers. Maybe use TOAST popups + an icon in the layers list to indicate bad layer.
        // When a layer has an error, don't draw/update it. WILL NEED special logic for if we're in preview mode.
        //TODO: Draw "selection box" over layer in preview.
        //TODO: Intro animations, Layout lifetimes, Outro animations.
        //TODO: Use command-based scripting as the back-end for *ALL* layer settings.
        //      This is important for fancy animations.
        //TODO: ErrorHandler should be for app-wide errors. Right now it relies on layerids.
        //TODO: Layers should be able to embed resources (files/streams) into the layout.
        //TODO: Support Undo/Redo in previewmode.
        //TODO: Support locking X/Y in previewmode.
        //TODO: Support incremental X/Y changes (ie, greater than 1 single pixel) in previewmode.


        //Notes:
        //A 'module' is a script/extension.
        //A 'controller' is an active instance of a module.
        //A 'layer' contains a module and various other settings.

        public LayoutTrackingService LayoutTracker { get; private set; }

        private GraphicsDeviceManager _graphicsManager;
        private SpriteBatch _spriteBatch;
        private Dictionary<string, Effect> _effectsCache;

        private Services.ControllerService _controllerService;

        private EditModeHandler _editModeHandler;

        private Form _form;

        public Game()
        {
            _graphicsManager = new GraphicsDeviceManager(this);
            //Load settings
            if (File.Exists("settings.json"))
            {
                Settings.Load("settings.json");
            }

            //Since the span of multiple monitors is probably too large, we use a scaling factor.
            //The scale factor defaults to 0.5. Which is enough to keep it from crashing on my system.
            //At a later time, this value will be changable by the user.
            _graphicsManager.PreferredBackBufferWidth = (int)(SystemInformation.VirtualScreen.Width * Settings.Instance.BackBufferWidthFactor);
            _graphicsManager.PreferredBackBufferHeight = (int)(SystemInformation.VirtualScreen.Height * Settings.Instance.BackBufferHeightFactor);

            //Set the frame rate.
            TargetElapsedTime = TimeSpan.FromSeconds(1.0F / Settings.Instance.FrameRate);


            //Allow the serviceprovider to go ahead and resolve services and servicereferences.
            ServiceProvider.Init();

        }

        protected override void Initialize()
        {
            base.Initialize();
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Initialize services
            _editModeHandler = ServiceProvider.GetService<EditModeHandler>();
            _editModeHandler.Init(_spriteBatch);

            //Load in extension modules.
            //This really just caches them for later use.
            ModuleCache.LoadModules(AppDomain.CurrentDomain.BaseDirectory + "modules\\");

            //Find the main layout service.
            LayoutTracker = ServiceProvider.GetService<LayoutTrackingService>();

            //Create the controller service.
            _controllerService = new ControllerService();

            //Setup the window by casting the game window to a windows forms control.
            _form = (Form)Control.FromHandle(Window.Handle);
            _form.FormBorderStyle = FormBorderStyle.None;
            _form.Height = SystemInformation.VirtualScreen.Height;
            _form.Width = SystemInformation.VirtualScreen.Width;
            _form.SetDesktopLocation(0, 0);

            //Set the window's parent to be the desktop.
            WindowHandler.SetParet(_form.Handle);

            //Instantiate the effects cache.
            _effectsCache = new Dictionary<string, Effect>();

            //Setup the content manager we'll use for effects loading.
            Content.RootDirectory = AppDomain.CurrentDomain.BaseDirectory;


            //Initialize the controllers the user has in the current layout.
            _controllerService.Reset();
        }


        public void ResetGraphicsSettings()
        {
            _graphicsManager.PreferredBackBufferWidth = (int)(SystemInformation.VirtualScreen.Width * Settings.Instance.BackBufferWidthFactor);
            _graphicsManager.PreferredBackBufferHeight = (int)(SystemInformation.VirtualScreen.Height * Settings.Instance.BackBufferHeightFactor);
            TargetElapsedTime = TimeSpan.FromSeconds(1.0F / Settings.Instance.FrameRate);
            _graphicsManager.ApplyChanges();


            //TODO: THIS BREAKS THINGS
            _controllerService.Reset();
        }

        protected override void Update(GameTime gameTime)
        {
            _editModeHandler.Update(gameTime);

            //Update enabled controllers.
            foreach (var controller in _controllerService.Controllers)
            {
                if (!controller.Settings.Enabled)
                {
                    continue;
                }
                controller.Update(gameTime);
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //DrawTest(gameTime);
            DrawLayers(gameTime);
            Present(gameTime);
            _editModeHandler.Draw(gameTime);
            base.Draw(gameTime);
        }

        private void DrawLayers(GameTime gameTime)
        {
            //Draw each controller onto its own rendertarget.
            foreach (var controller in _controllerService.Controllers)
            {
                if (!controller.Settings.Enabled)
                {
                    continue;
                }
                GraphicsDevice.SetRenderTarget(controller.Rendering.RenderTarget);
                GraphicsDevice.Clear(Color.Transparent);
                controller.Draw(gameTime);
            }
            GraphicsDevice.SetRenderTarget(null);
        }

        private void Present(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            string lastEffect = "[n/a]";
            bool beginCalled = false;

            //Draw each controller's rendertarget onto the screen.
            foreach (var controller in _controllerService.Controllers)
            {
                if (!controller.Settings.Enabled)
                {
                    continue;
                }

                if (controller.Settings.Effect != lastEffect)
                {
                    if (beginCalled)
                    {
                        _spriteBatch.End();
                    }
                    if (controller.Settings.Effect == "" || controller.Settings.Effect.EndsWith("[Default].xnb"))
                    {
                        lastEffect = "";
                        _spriteBatch.Begin();
                        beginCalled = true;
                    }
                    else
                    {
                        lastEffect = controller.Settings.Effect;
                        _spriteBatch.Begin(effect: _effectsCache[controller.Settings.Effect]);
                        beginCalled = true;
                    }
                }

                //Get the dimensions of the layer, applying the scale factor.
                var rect = controller.Settings.Dimensions.GetBoundsRectangle();
                var position = new Vector2(rect.X * Settings.Instance.BackBufferWidthFactor, rect.Y * Settings.Instance.BackBufferHeightFactor);
                var scale = new Vector2(Settings.Instance.BackBufferWidthFactor, Settings.Instance.BackBufferHeightFactor);
                scale = Vector2.One;

                rect.X = rect.X * Settings.Instance.BackBufferWidthFactor;
                rect.Y = rect.Y * Settings.Instance.BackBufferHeightFactor;
                //rect.Width = (int)(rect.Width * Settings.Instance.BackBufferWidthFactor);
                //rect.Height = (int)(rect.Height * Settings.Instance.BackBufferHeightFactor);


                //Draw the controller.

                //Vector2 originVector = new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                //_spriteBatch.Draw(controller.RenderTarget, rect, null, controller.Settings.TintColor, (float) (controller.Settings.Rotation * RADIAN_MULTIPLIER), originVector, SpriteEffects.None, 0.0F);

                Console.WriteLine($"Drawing at X: {rect.X}, Y: {rect.Y}");
                _spriteBatch.Draw(controller.Rendering.RenderTarget, position, null, controller.Settings.TintColor, 0.0F, Vector2.Zero, scale, SpriteEffects.None, 0.0F);
            }
            if (beginCalled)
            {
                _spriteBatch.End();
            }
        }
    }
}
