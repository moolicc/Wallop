using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WallApp.Scripting;
using WallApp.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WallApp.UI.ViewModels;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SystemInformation = System.Windows.Forms.SystemInformation;

namespace WallApp
{
    class Game : Microsoft.Xna.Framework.Game
    {
        private const double RADIAN_MULTIPLIER = 0.0174532925199433D;

        //Notes:
        //A 'module' is a script/extension.
        //A 'controller' is an active instance of a module.
        //A 'layer' contains a module and various other settings.

        private GraphicsDeviceManager _graphicsManager;
        private SpriteBatch _spriteBatch;
        private Dictionary<string, Effect> _effectsCache;
    
        private List<Controller> _controllers;

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

            //Load in extension modules.
            //This really just caches them for later use.
            Resolver.LoadModules(AppDomain.CurrentDomain.BaseDirectory + "modules\\");




            var win = new WallApp.UI.Views.SettingsWindow(new SettingsViewModel(new SettingsModel()));
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

            //Load the layout
            if (!File.Exists("layout.json"))
            {
                Layout.New();
                Layout.Save("layout.json");
            }
            else
            {
                Layout.Load("layout.json");
            }

            //Instantiate the effects cache.
            _effectsCache = new Dictionary<string, Effect>();
            
            //Show the settings window.
            ShowSettings();

            //Initialize the controllers the user has in the current layout.
            InitializeControllers();
        }

        private void ShowSettings()
        {
            //Show the settings window.
            SettingsWindow window = new SettingsWindow();
            if(window.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            //Apply settings.
            if (window.SettingsChanged)
            {
                _graphicsManager.PreferredBackBufferWidth = (int)(SystemInformation.VirtualScreen.Width * Settings.Instance.BackBufferWidthFactor);
                _graphicsManager.PreferredBackBufferHeight = (int)(SystemInformation.VirtualScreen.Height * Settings.Instance.BackBufferHeightFactor);
                TargetElapsedTime = TimeSpan.FromSeconds(1.0F / Settings.Instance.FrameRate);
                _graphicsManager.ApplyChanges();
            }
            
            //Apply new layout.
            if (window.LayoutChanged)
            {
                if (_controllers != null)
                {
                    //Dispose old controller rendertargets.
                    foreach (var controller in _controllers)
                    {
                        controller.Rendering.RenderTarget.Dispose();
                    }

                    _controllers.Clear();
                }
                //Initialize the new layout.
                InitializeControllers();
            }
        }

        private void InitializeControllers()
        {
            //Setup our controller list.
            _controllers = new List<Controller>();

            foreach (var layer in Layout.Layers)
            {
                //Get the current layer's module out of the cache. This creates a clone instead of referencing
                //the module in the layer.
                var module = Resolver.Cache[layer.Module];

                //Create the controller to be used.
                var controller = module.CreateController();

                //Get the user-specified dimensions of the layer.
                (int x, int y, int width, int height) = layer.Dimensions.GetBounds();

                //Pass the layer's configuration to the controller.
                controller.Settings = layer;
                controller.Module = module;
                var scaledLayerBounds = new Rectangle(
                    (int) (x * Settings.Instance.BackBufferWidthFactor),
                    (int) (y * Settings.Instance.BackBufferHeightFactor),
                    (int) (width * Settings.Instance.BackBufferWidthFactor),
                    (int) (height * Settings.Instance.BackBufferHeightFactor));
                controller.PlaceControl = c =>
                {
                    if (!c.Bounds.Contains(new System.Drawing.Rectangle(scaledLayerBounds.X,
                        scaledLayerBounds.Y, scaledLayerBounds.Width,
                        scaledLayerBounds.Height)))
                    {
                        return;
                    }
                    _form.Controls.Add(c);
                };

                //We setup the rendertarget that the controller will draw to.
                var renderTarget = new RenderTarget2D(GraphicsDevice, width, height);

                //Init the controller's rendering parameters.
                controller.Rendering = new Rendering(GraphicsDevice, renderTarget);
                controller.Rendering.ActualX = scaledLayerBounds.X;
                controller.Rendering.ActualY = scaledLayerBounds.Y;
                controller.Rendering.ActualWidth = scaledLayerBounds.Width;
                controller.Rendering.ActualHeight = scaledLayerBounds.Height;

                //Allow the controller to handle any initialization is needs.
                controller.Setup();
                
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
            DrawLayers(gameTime);
            Present(gameTime);
            base.Draw(gameTime);
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
                    if (controller.Settings.Effect == "")
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
                rect.X = (int) (rect.X * Settings.Instance.BackBufferWidthFactor);
                rect.Y = (int)(rect.Y * Settings.Instance.BackBufferHeightFactor);
                rect.Width = (int)(rect.Width * Settings.Instance.BackBufferWidthFactor);
                rect.Height = (int)(rect.Height * Settings.Instance.BackBufferHeightFactor);

                //Draw the controller.

                //Vector2 originVector = new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                //_spriteBatch.Draw(controller.RenderTarget, rect, null, controller.Settings.TintColor, (float) (controller.Settings.Rotation * RADIAN_MULTIPLIER), originVector, SpriteEffects.None, 0.0F);
                
                _spriteBatch.Draw(controller.Rendering.RenderTarget, rect, controller.Settings.TintColor);
            }
            _spriteBatch.End();
        }
    }
}
