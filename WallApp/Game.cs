using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using WallApp.Scripting;
using WallApp.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SystemInformation = System.Windows.Forms.SystemInformation;

namespace WallApp
{
    class Game : Microsoft.Xna.Framework.Game
    {
        //Notes:
        //A 'module' is a script/extension.
        //A 'controller' is an active instance of a module.
        //A 'layer' contains a module and various other settings.

        private GraphicsDeviceManager _graphicsManager;
        private SpriteBatch _spriteBatch;

        private Settings _settings;
        private List<Controller> _controllers;

        private Form _form;

        public Game()
        {
            _settings = new Settings();
            _graphicsManager = new GraphicsDeviceManager(this);

            //Since the span of multiple monitors is probably too large, we use a scaling factor.
            //The scale factor defaults to 0.5. Which is enough to keep it from crashing on my system.
            //At a later time, this value will be changable by the user.
            _graphicsManager.PreferredBackBufferWidth = (int)(SystemInformation.VirtualScreen.Width * _settings.BackBufferWidthFactor);
            _graphicsManager.PreferredBackBufferHeight = (int)(SystemInformation.VirtualScreen.Height * _settings.BackBufferHeightFactor);

            //Load in extension modules.
            //This really just caches them for later use.
            Resolver.LoadModules(AppDomain.CurrentDomain.BaseDirectory + "modules\\");
        }

        protected override void Initialize()
        {
            base.Initialize();
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Set the window's parent to be the desktop.
            WindowHandler.SetParet(Window.Handle);

            //Setup the window through the WindowsForms API.
            _form = (Form)Control.FromHandle(Window.Handle);
            _form.FormBorderStyle = FormBorderStyle.None;
            _form.SetDesktopLocation(0, 0);
            _form.Width = SystemInformation.VirtualScreen.Width;
            _form.Height = SystemInformation.VirtualScreen.Height;

            //Show the settings window.
            SettingsWindow window = new SettingsWindow();
            window.LayerLayout = new Layout(); //Hardcoded initialization, will change later.
            window.ShowDialog();

            //Initialize the controllers the user enabled in the settings window.
            InitializeControllers(window.LayerLayout);
        }

        private void InitializeControllers(Layout layout)
        {
            //Setup our controller list.
            _controllers = new List<Controller>();

            foreach (var layer in layout.Layers)
            {
                //Get the current layer's module out of the cache. This creates a clone instead of referencing
                //the module in the layer.
                var module = Resolver.Cache[layer.Module];

                //Create the controller to be used.
                var controller = module.CreateController();

                //Get the user-specified dimensions of the layer.
                (int x, int y, int width, int height) = layer.Dimensions.GetBounds();

                //Pass the layer's configuration to the controller.
                controller.LayerSettings = layer;
                controller.GraphicsDevice = GraphicsDevice;
                controller.Module = module;
                controller.ScaledLayerBounds = new Rectangle(
                    (int) (x * _settings.BackBufferWidthFactor),
                    (int) (y * _settings.BackBufferHeightFactor),
                    (int) (width * _settings.BackBufferWidthFactor),
                    (int) (height * _settings.BackBufferHeightFactor));
                controller.PlaceControl = c =>
                {
                    if (!c.Bounds.Contains(new System.Drawing.Rectangle(controller.ScaledLayerBounds.X,
                        controller.ScaledLayerBounds.Y, controller.ScaledLayerBounds.Width,
                        controller.ScaledLayerBounds.Height)))
                    {
                        return;
                    }
                    _form.Controls.Add(c);
                };

                //We setup the rendertarget that the controller will draw to.
                controller.RenderTarget = new RenderTarget2D(GraphicsDevice, width, height);

                //Allow the controller to handle any initialization is needs.
                controller.Setup();
                
                _controllers.Add(controller);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            //Update enabled controllers.
            foreach (var controller in _controllers)
            {
                if (!controller.LayerSettings.Enabled)
                {
                    continue;
                }
                controller.Update(gameTime);
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            DrawLayers(gameTime);
            Present(gameTime);
            base.Draw(gameTime);
        }

        private void DrawLayers(GameTime gameTime)
        {
            //Draw each controller onto its own rendertarget.
            foreach (var controller in _controllers)
            {
                if (!controller.LayerSettings.Enabled)
                {
                    continue;
                }
                GraphicsDevice.SetRenderTarget(controller.RenderTarget);
                GraphicsDevice.Clear(Color.Transparent);
                controller.Draw(gameTime);
            }
            GraphicsDevice.SetRenderTarget(null);
        }

        private void Present(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            //Draw each controller's rendertarget onto the screen.
            foreach (var controller in _controllers)
            {
                if (!controller.LayerSettings.Enabled)
                {
                    continue;
                }
                //Get the dimensions of the layer, applying the scale factor.
                var rect = controller.LayerSettings.Dimensions.GetBoundsRectangle();
                rect.X = (int) (rect.X * _settings.BackBufferWidthFactor);
                rect.Y = (int)(rect.Y * _settings.BackBufferHeightFactor);
                rect.Width = (int)(rect.Width * _settings.BackBufferWidthFactor);
                rect.Height = (int)(rect.Height * _settings.BackBufferHeightFactor);
                
                //Draw the controller.
                _spriteBatch.Draw(controller.RenderTarget, rect, controller.LayerSettings.TintColor);
            }
            _spriteBatch.End();
        }
    }
}
