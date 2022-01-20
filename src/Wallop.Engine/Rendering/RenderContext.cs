using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Wallop.Engine.Rendering
{
    public class RenderContext
    {
        public CommandList Commands { get; init; }
        public GraphicsDevice Device { get; init; }

        public RenderContext(GraphicsManager graphics)
            : this(graphics.GraphicsDevice, graphics.Resources.CreateCommandList())
        {
        }

        public RenderContext(GraphicsManager graphics, CommandListDescription commandListDescription)
            : this(graphics.GraphicsDevice, graphics.Resources.CreateCommandList(commandListDescription))
        {
        }

        internal RenderContext(GraphicsDevice device, CommandList commandList)
        {
            Commands = commandList;
            Device = device;
        }
    }
}
