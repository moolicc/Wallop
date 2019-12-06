using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WallApp.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SystemInformation = System.Windows.Forms.SystemInformation;
using System.Windows;
using ServiceProvider = WallApp.Services.ServiceProvider;
using Hardcodet.Wpf.TaskbarNotification;
using WallApp.UI;



/*
 * All layout logic needs to come from the bridge.
 * All the ui needs to go away
 * The scripting interface needs to go away
 *
 * General refactoring
 *
 * When the engine gets the "createlayer" message, it needs to go ahead and create a controller for that layer.
 */



namespace WallApp
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
        public Layout Layout { get; private set; }

        private GraphicsDeviceManager _graphicsManager;
        private SpriteBatch _spriteBatch;
        private Dictionary<string, Effect> _effectsCache;

        private List<Controller> _controllers;

        private PreviewModeHandler _previewModeHandler;

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
            _previewModeHandler = ServiceProvider.GetService<PreviewModeHandler>();
            _previewModeHandler.Init(_spriteBatch);

            //Load in extension modules.
            //This really just caches them for later use.
            Resolver.LoadModules(AppDomain.CurrentDomain.BaseDirectory + "modules\\");

            //Find the main layout service.
            Layout = ServiceProvider.GetService<Layout>("/main");

            /* FOR USE WITH SDL2_CS
            //Setup the window through the SDL API
            var width = SystemInformation.VirtualScreen.Width;
            var height = SystemInformation.VirtualScreen.Height;

            //Set the size, position, and borders of the window.
            SDL.SDL_SetWindowSize(Window.Handle, width, height);
            SDL.SDL_SetWindowPosition(Window.Handle, SystemInformation.VirtualScreen.Left, SystemInformation.VirtualScreen.Top);
            SDL.SDL_SetWindowBordered(Window.Handle, SDL.SDL_bool.SDL_FALSE);

            //Get the HWND from SDL.
            SDL.SDL_GetVersion(out var sdlVersion);
            var info = new SDL.SDL_SysWMinfo();
            info.version = sdlVersion;
            SDL.SDL_GetWindowWMInfo(Window.Handle, ref info);

            var f = (Form) Control.FromHandle(info.info.win.window);
            //Set the window's parent to be the desktop.
            WindowHandler.SetParet(f.Handle);
            */

            //Setup the window by casting the game window to a windows forms control.
            _form = (Form) Control.FromHandle(Window.Handle);
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
            InitializeControllers();
        }


        public void ResetGraphicsSettings()
        {
            _graphicsManager.PreferredBackBufferWidth = (int)(SystemInformation.VirtualScreen.Width * Settings.Instance.BackBufferWidthFactor);
            _graphicsManager.PreferredBackBufferHeight = (int)(SystemInformation.VirtualScreen.Height * Settings.Instance.BackBufferHeightFactor);
            TargetElapsedTime = TimeSpan.FromSeconds(1.0F / Settings.Instance.FrameRate);
            _graphicsManager.ApplyChanges();


            //TODO: THIS BREAKS THINGS
            InitNewLayout();
        }

        public void InitNewLayout()
        {
            if (_controllers != null)
            {
                //Dispose old controller rendertargets.
                foreach (var controller in _controllers)
                {
                    controller.Dispose();
                    controller.Rendering.RenderTarget.Dispose();
                }

                _controllers.Clear();
            }
            //Initialize the new layout.
            InitializeControllers();
        }

        private void InitializeControllers()
        {
            //Setup our controller list.
            _controllers = new List<Controller>();

            foreach (var layer in Layout.Layers)
            {
                //Get the current layer's module out of the cache. This creates a clone instead of referencing
                //the module in the layer.

                //TODO: This is bad. layer.Module is supposed to be the module's FULL filepath, but UI broke that.
                //In the future the UI should also deal in absolute paths, but transform the text when displayed to
                //just filenames.
                var module = Resolver.GetCachedModuleFromName(layer.Module);

                //Create the controller to be used.
                var controller = module.CreateController();

                //Get the user-specified dimensions of the layer.
                (float x, float y, float width, float height) = layer.Dimensions.GetBounds();

                //Pass the layer's configuration to the controller.
                controller.Settings = layer;
                controller.Module = module;
                var scaledLayerBounds = new RectangleF(
                    (x * Settings.Instance.BackBufferWidthFactor),
                    (y * Settings.Instance.BackBufferHeightFactor),
                    (width * Settings.Instance.BackBufferWidthFactor),
                    (height * Settings.Instance.BackBufferHeightFactor));

                //We setup the rendertarget that the controller will draw to.
                var renderTarget = new RenderTarget2D(GraphicsDevice, (int)scaledLayerBounds.Width, (int)scaledLayerBounds.Height);

                //Init the controller's rendering parameters.
                controller.Rendering = new Rendering(GraphicsDevice, renderTarget);
                controller.Rendering.ActualX = (int)scaledLayerBounds.X;
                controller.Rendering.ActualY = (int)scaledLayerBounds.Y;
                controller.Rendering.ActualWidth = (int)scaledLayerBounds.Width;
                controller.Rendering.ActualHeight = (int)scaledLayerBounds.Height;

                //Allow the controller to handle any initialization is needs.
                try
                {
                    controller.Setup();
                }
                catch (Exception ex)
                {
                }

                //Cache any effects.
                if (!string.IsNullOrWhiteSpace(layer.Effect))
                {
                    if (!_effectsCache.ContainsKey(layer.Effect))
                    {
                        _effectsCache.Add(layer.Effect, Effects.CreateEffect(layer.Effect, Content));
                    }
                }

                _controllers.Add(controller);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            _previewModeHandler.Update(gameTime);

            //Update enabled controllers.
            foreach (var controller in _controllers)
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
            _previewModeHandler.Draw(gameTime);
            base.Draw(gameTime);
        }

        private void DrawTest(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            foreach (var controller in _controllers)
            {
                if (!controller.Settings.Enabled)
                {
                    continue;
                }
                controller.Draw(gameTime);
            }
        }

        private void DrawLayers(GameTime gameTime)
        {
            //Draw each controller onto its own rendertarget.
            foreach (var controller in _controllers)
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
            foreach (var controller in _controllers)
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
            if(beginCalled)
            {
                _spriteBatch.End();
            }
        }
    }
}
