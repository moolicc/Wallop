﻿using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Messaging;
using Wallop.Engine.Settings;
using Wallop.Engine.Types.Plugins;
using Wallop.Engine.Types.Plugins.EndPoints;

namespace Wallop.Engine.Handlers
{
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

        public override Command? GetCommandLineCommand(bool firstInstance)
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
                () => _graphicsSettings.SkipOverlay,
                "Specifies whether or not to overlay the window over the desktop.");
            var refreshRateOpt = new Option<double>(
                new[] { "--refresh-rate", "-r" },
                () => _graphicsSettings.RefreshRate,
                "Specifies the refresh rate.");
            var vsyncEnable = new Option<bool>(
                new[] { "--vsync", "-v" },
                () => _graphicsSettings.VSync,
                "Specifies if vsync should be enabled or disabled.");



            // EngineApp.exe graphics ...
            var graphicsCommand = new Command("graphics", "Graphics operations")
            {
                windowWidth,
                windowHeight,
                windowBorder,
                skipOverlayOpt,
                refreshRateOpt,
                vsyncEnable,
            };

            var graphicsHandler = CommandHandler.Create<int, int, WindowBorder, bool, double, bool>(
                (winWidth, winHeight, winBorder, overlay, refreshRate, vsync) =>
                {
                    var changes = new Settings.GraphicsSettings();
                    changes.WindowWidth = winWidth;
                    changes.WindowHeight = winHeight;
                    changes.WindowBorder = winBorder;
                    changes.SkipOverlay = overlay;
                    changes.RefreshRate = refreshRate;
                    changes.VSync = vsync;

                    if (WindowInitialized || !firstInstance)
                    {
                        App.Messenger.Put(new GraphicsMessage() { ChangeSet = changes });
                    }
                    else
                    {
                        _graphicsSettings = changes;
                    }
                });

            graphicsCommand.Handler = graphicsHandler;
            return graphicsCommand;
        }

        public void HandleGraphicsMessage(GraphicsMessage msg, uint messageId)
        {
            _graphicsSettings.WindowWidth = msg.ChangeSet.WindowWidth;
            _graphicsSettings.WindowHeight = msg.ChangeSet.WindowHeight;
            _graphicsSettings.WindowBorder = msg.ChangeSet.WindowBorder;
            _graphicsSettings.RefreshRate = msg.ChangeSet.RefreshRate;
            _graphicsSettings.VSync = msg.ChangeSet.VSync;

            _window.VSync = _graphicsSettings.VSync;
            _window.WindowBorder = _graphicsSettings.WindowBorder;
            _window.Size = new Vector2D<int>(_graphicsSettings.WindowWidth, _graphicsSettings.WindowHeight);
            _window.UpdatesPerSecond = _graphicsSettings.RefreshRate;
            _window.FramesPerSecond = _graphicsSettings.RefreshRate;

            if (_graphicsSettings.SkipOverlay != msg.ChangeSet.SkipOverlay)
            {
                // Toggle overlay.
            }
            _graphicsSettings.SkipOverlay = msg.ChangeSet.SkipOverlay;

            UpdateGraphics();
        }

        public void RunWindow()
        {
            var pluginContext = App.GetService<PluginPantry.PluginContext>().OrThrow();
            EngineLog.For<GraphicsHandler>().Debug("Executing plugins on EngineStartup...");
            pluginContext.ExecuteEndPoint(new EngineStartupEndPoint { GraphicsSettings = _graphicsSettings });


            EngineLog.For<GraphicsHandler>().Info("Creating window with options: {options}", _graphicsSettings);
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(_graphicsSettings.WindowWidth, _graphicsSettings.WindowHeight);
            options.WindowBorder = _graphicsSettings.WindowBorder;
            options.IsVisible = _graphicsSettings.SkipOverlay;
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

            WindowInitialized = true;

            _window.Run();
        }

        public void ClearSurface()
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
        }

        private void WindowLoad()
        {
            var pluginContext = App.GetService<PluginPantry.PluginContext>().OrThrow();
            _window.GLContext.MakeCurrent();
            _gl = GL.GetApi(_window);
            

            var GLMajorVersion = _gl.GetInteger(GLEnum.MajorVersion);
            var GLMinorVersion = _gl.GetInteger(GLEnum.MinorVersion);
            EngineLog.For<GraphicsHandler>().Info("OpenGL Initialized. Version {major}.{minor}.", GLMajorVersion, GLMinorVersion);



            if (!_graphicsSettings.SkipOverlay)
            {
                EngineLog.For<GraphicsHandler>().Info("Running execution of Engine Overlay plugin...");
                pluginContext.ExecuteEndPoint(new OverlayerEndPoint(_window));
                pluginContext.WaitForEndPointExecutionAsync<OverlayerEndPoint>().ContinueWith(_ =>
                {
                    _window.IsVisible = true;
                });
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
            UpdateGraphics(size);
        }

        private void UpdateGraphics(Vector2D<int>? size = null)
        {
            if(size == null)
            {
                _gl.Viewport(_window.Size);
            }
            else
            {
                _gl.Viewport(size.Value);
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
