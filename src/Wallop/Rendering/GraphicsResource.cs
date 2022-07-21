using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Rendering
{
    internal abstract class GraphicsResource : IDisposable
    {
        public string? ResourceName { get; set; }
        public bool IsDisposed { get; private set; }
        public GraphicsDevice? GraphicsDevice { get; private set; }

        protected GraphicsResource()
        {
        }

        protected GraphicsResource(string? resourceName)
        {
            ResourceName = resourceName;
        }


        ~GraphicsResource()
        {
            if(GraphicsDevice != null && !IsDisposed)
            {
                Dispose(false);
            }
        }

        public GraphicsDevice GetGraphicsDeviceOrThrow()
        {
            if(GraphicsDevice == null)
            {
                throw new ArgumentNullException(nameof(GraphicsDevice), "GraphicsDevice not bound!");
            }
            return GraphicsDevice;
        }

        public void Bind(GraphicsDevice device)
        {
            if(GraphicsDevice != null)
            {
                throw new InvalidOperationException("This resource has already been bound to a particular GraphicsDevice.");
            }
            GraphicsDevice = device;
            DeviceBound(device);
        }

        protected void CheckNotDisposed()
        {
            if(IsDisposed)
            {
                throw new ObjectDisposedException(ResourceName, "Graphics resource has been disposed.");
            }
        }

        protected void CheckGraphicsDeviceBound()
        {
            if(GraphicsDevice == null)
            {
                throw new NullReferenceException("Graphics resource has not been bound to a GraphicsDevice.");
            }
        }

        public void Dispose()
        {
            if(!IsDisposed)
            {
                Dispose(true);
                IsDisposed = true;
                GraphicsDevice = null;
            }
        }

        protected abstract void DeviceBound(GraphicsDevice device);
        protected abstract void Dispose(bool disposing);
    }
}
