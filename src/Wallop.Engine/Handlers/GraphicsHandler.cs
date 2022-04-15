using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Messaging;

namespace Wallop.Engine.Handlers
{
    // TODO: Should move ALL graphics related setup and modifying to here.
    internal class GraphicsHandler : EngineHandler
    {
        public GraphicsHandler(EngineApp engineInstance)
            : base(engineInstance)
        {
            SubscribeToEngineMessages<GraphicsMessage>(HandleGraphicsMessage);
        }

        public void HandleGraphicsMessage(GraphicsMessage msg, uint messageId)
        {
            App.GraphicsSettings.WindowWidth = msg.ChangeSet.WindowWidth;
            App.GraphicsSettings.WindowHeight = msg.ChangeSet.WindowHeight;
            App.GraphicsSettings.WindowBorder = msg.ChangeSet.WindowBorder;
            App.GraphicsSettings.RefreshRate = msg.ChangeSet.RefreshRate;
            App.GraphicsSettings.VSync = msg.ChangeSet.VSync;

            App.Window.VSync = App.GraphicsSettings.VSync;
            App.Window.WindowBorder = App.GraphicsSettings.WindowBorder;
            App.Window.Size = new Vector2D<int>(App.GraphicsSettings.WindowWidth, App.GraphicsSettings.WindowHeight);
            App.Window.UpdatesPerSecond = App.GraphicsSettings.RefreshRate;
            App.Window.FramesPerSecond = App.GraphicsSettings.RefreshRate;

            if (App.GraphicsSettings.SkipOverlay != msg.ChangeSet.SkipOverlay)
            {
                // Toggle overlay.
            }
            App.GraphicsSettings.SkipOverlay = msg.ChangeSet.SkipOverlay;

            App.UpdateGraphics();
        }

        internal GL GetGlIsntance()
        {
            throw new NotImplementedException();
        }
    }
}
