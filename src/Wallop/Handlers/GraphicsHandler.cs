using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Messaging;
using Wallop.Shared.Messaging.Messages;
using Wallop.Settings;
using Wallop.Types.Plugins;
using Wallop.Types.Plugins.EndPoints;
using Silk.NET.Windowing.Sdl;
using Silk.NET.Core.Contexts;

namespace Wallop.Handlers
{
    // TODO: Use libvlc to support video playback
    // TODO: Should move ALL graphics related setup and modifying to here.
    internal class GraphicsHandler : EngineHandler
    {
        public bool WindowInitialized { get; private set; }

        private GL _gl;
        private IWindow _window;
        private GraphicsSettings _graphicsSettings;

        public GraphicsHandler(EngineApp engineInstance, GraphicsSettings graphicsSettings) : base(engineInstance)
        {
            SubscribeToEngineMessages<GraphicsMessage>(HandleGraphicsMessage);
            _graphicsSettings = graphicsSettings;
        }

        public override Command? GetCommandLineCommand()
        {
            var windowWidth = new Option<int>(new[] { "--win-width", "-w" },
                () => _graphicsSettings.WindowWidth,
                "Specifies the width of the underlying window.");
            var windowHeight = new Option<int>(
                new[] { "--win-height", "-h" },
                () => _graphicsSettings.WindowHeight,
                "Specifies the height of the underlying window.");
            var windowBorder = new Option<WindowBorder>(
                new[] { "--win-border", "-b" },
                () => _graphicsSettings.WindowBorder,
                "Specifies the border style of the underlying window.");
            var skipOverlayOpt = new Option<bool>(
                new[] { "--overlay", "-o" },
                () => _graphicsSettings.Overlay,
                "Specifies whether or not to overlay the window over the desktop.");
            var refreshRateOpt = new Option<double>(
                new[] { "--refresh-rate", "-r" },
                () => _graphicsSettings.RefreshRate,
                "Specifies the refresh rate.");
            var vsyncEnable = new Option<bool>(
                new[] { "--vsync", "-v" },
                () => _graphicsSettings.VSync,
                "Specifies if vsync should be enabled or disabled.");
            var parentOption = new Option<string>(
                new[] { "--parent", "-p" },
                () => "",
                "Specifies the handle for the window to render onto.");



            // EngineApp.exe graphics ...
            var graphicsCommand = new Command("graphics", "Graphics operations")
            {
                windowWidth,
                windowHeight,
                windowBorder,
                skipOverlayOpt,
                refreshRateOpt,
                vsyncEnable,
                parentOption
            };

            var graphicsHandler = CommandHandler.Create<int, int, WindowBorder, bool, double, bool, string>(
                (winWidth, winHeight, winBorder, overlay, refreshRate, vsync, parent) =>
                {

                    if (WindowInitialized)
                    {
                        App.Messenger.Put(new GraphicsMessage(winWidth, winHeight, overlay, (int)winBorder, refreshRate, vsync, parent));
                    }
                    else
                    {
                        var changes = new GraphicsSettings();
                        changes.WindowWidth = winWidth;
                        changes.WindowHeight = winHeight;
                        changes.WindowBorder = winBorder;
                        changes.Overlay = overlay;
                        changes.RefreshRate = refreshRate;
                        changes.VSync = vsync;
                        changes.ParentHandle = parent;

                        _graphicsSettings = changes;
                    }
                });

            graphicsCommand.Handler = graphicsHandler;
            return graphicsCommand;
        }

        public object HandleGraphicsMessage(GraphicsMessage msg, uint messageId)
        {
            try
            {
                (int? X, int? Y) newSize = (msg.WindowWidth, msg.WindowHeight);
                var newBorder = (WindowBorder?)msg.WindowBorder;
                var newRefreshRate = msg.RefreshRate;
                var newVsync = msg.VSync;


                bool? overlayUpdate = null;
                if (msg.Overlay.HasValue && _graphicsSettings.Overlay != msg.Overlay.Value)
                {
                    overlayUpdate = msg.Overlay;
                }

                UpdateGraphics(newSize, newBorder, newRefreshRate, newVsync, overlayUpdate);
            }
            catch (Exception ex)
            {
                return Fail(messageId, ex);
            }

            return Success(messageId, _graphicsSettings);
        }

        public void ShowWindow()
        {
            _window.IsVisible = true;
        }

        public void RunWindow()
        {
            var pluginContext = App.GetService<PluginPantry.PluginContext>().OrThrow();
            EngineLog.For<GraphicsHandler>().Debug("Executing plugins on EngineStartup...");
            //pluginContext.ExecuteEndPoint(new EngineStartupEndPoint { GraphicsSettings = _graphicsSettings });


            EngineLog.For<GraphicsHandler>().Info("Creating window with options: {options}", _graphicsSettings);

            /*
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(_graphicsSettings.WindowWidth, _graphicsSettings.WindowHeight);
            options.WindowBorder = _graphicsSettings.WindowBorder;
            options.IsVisible = _graphicsSettings.Overlay;
            options.FramesPerSecond = _graphicsSettings.RefreshRate;
            options.UpdatesPerSecond = _graphicsSettings.RefreshRate;
            options.VSync = _graphicsSettings.VSync;
            options.ShouldSwapAutomatically = true;


            Window.PrioritizeSdl();
            _window = Window.Create(options);
            _window.Load += WindowLoad;
            _window.FramebufferResize += WindowResized;
            _window.Update += App.Update;
            _window.Render += App.Draw;
            _window.Closing += App.Shutdown;
            */

            _graphicsSettings.ParentHandle = "";
            bool newWindow = string.IsNullOrEmpty(_graphicsSettings.ParentHandle.Trim().Trim('"'));
            if (newWindow)
            {
                Window.PrioritizeSdl();
                _window = Window.Create(WindowOptions.Default);
            }
            else
            {
                unsafe
                {
                    var handle = IntPtr.Parse(_graphicsSettings.ParentHandle).ToPointer();
                    _window = (IWindow)SdlWindowing.CreateFrom(handle);
                }
            }

            if (newWindow)
            {
                _window.Size = new Vector2D<int>(_graphicsSettings.WindowWidth, _graphicsSettings.WindowHeight);
                _window.WindowBorder = _graphicsSettings.WindowBorder;
                _window.IsVisible = !_graphicsSettings.Overlay;
            }
            _window.FramesPerSecond = _graphicsSettings.RefreshRate;
            _window.UpdatesPerSecond = _graphicsSettings.RefreshRate;
            _window.VSync = _graphicsSettings.VSync;
            _window.ShouldSwapAutomatically = true;

            _window.Load += WindowLoad;
            _window.FramebufferResize += WindowResized;
            _window.Update += App.Update;
            _window.Render += App.Draw;
            _window.Closing += App.Shutdown;

            if (newWindow)
            {
                _window.Run();
            }
            else
            {
                WindowLoad();
            }
        }

        public void ClearSurface()
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
        }

        private void WindowLoad()
        {
            var pluginContext = App.GetService<PluginPantry.PluginContext>().OrThrow();
            _gl = _window.CreateOpenGL();
            _window.GLContext.MakeCurrent();
            

            var GLMajorVersion = _gl.GetInteger(GLEnum.MajorVersion);
            var GLMinorVersion = _gl.GetInteger(GLEnum.MinorVersion);
            EngineLog.For<GraphicsHandler>().Info("OpenGL Initialized. Version {major}.{minor}.", GLMajorVersion, GLMinorVersion);


            var allScreenBounds = Types.ScreenInfo.GetVirtualScreen().Bounds;
            Console.WriteLine("Virtual screen bounds: {{ {0}, {1}, {2}, {3} }}", allScreenBounds.X, allScreenBounds.Y, allScreenBounds.Z, allScreenBounds.W);
            

            if (_graphicsSettings.Overlay)
            {
                EngineLog.For<GraphicsHandler>().Info("Running execution of Engine Overlay plugin...");
                pluginContext.RunAction(new OverlayerEndPoint(App.Messenger, _window));
                _window.IsVisible = true;
            }
            else
            {
                EngineLog.For<GraphicsHandler>().Info("Skipping execution of Engine Overlay plugin due to settings specified in configuration.");
            }

            App.WindowLoaded();
            _gl.Viewport(_window.Size);
        }

        private void WindowResized(Vector2D<int> size)
        {
            UpdateGraphics((size.X, size.Y));
        }

        private void UpdateGraphics((int? X, int? Y)? size = null, WindowBorder? border = null, double? refreshRate = null, bool? vsync = null, bool? overlay = null)
        {
            if (!size.HasValue)
            {
                _gl.Viewport(_window.Size);
            }
            else
            {
                var x = size.Value.X ?? _graphicsSettings.WindowWidth;
                var y = size.Value.Y ?? _graphicsSettings.WindowHeight;
                var newSize = new Vector2D<int>(x, y);

                _window.Size = newSize;
                _gl.Viewport(newSize);
            }

            if(border.HasValue)
            {
                _graphicsSettings.WindowBorder = border.Value;
                _window.WindowBorder = border.Value;
            }

            if(refreshRate.HasValue)
            {
                _graphicsSettings.RefreshRate = refreshRate.Value;
                _window.UpdatesPerSecond = refreshRate.Value;
                _window.FramesPerSecond = refreshRate.Value;
            }

            if(vsync.HasValue)
            {
                _graphicsSettings.VSync = vsync.Value;
                _window.VSync = vsync.Value;
            }


            if (overlay.GetValueOrDefault(false))
            {
                _graphicsSettings.Overlay = true;
                _graphicsSettings.WindowBorder = WindowBorder.Hidden;
                _window.WindowBorder = WindowBorder.Hidden;

                var pluginContext = App.GetService<PluginPantry.PluginContext>().OrThrow();
                EngineLog.For<GraphicsHandler>().Info("Running execution of Engine Overlay plugin...");

                pluginContext.RunAction(new OverlayerEndPoint(App.Messenger, _window));
                _window.IsVisible = true;
            }
        }

        public void Bump()
        {
            UpdateGraphics();
        }

        internal GL GetGlIsntance()
        {
            return _gl;
        }

        public override void Shutdown()
        {
            EngineLog.For<GraphicsHandler>().Info("GraphicsHandler shutdown.");
            if (!_window.IsClosing)
            {
                _window.Closing -= App.Shutdown;
                _window.Close();
                return;
            }
        }
    }
}
