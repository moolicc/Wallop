using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Rendering
{
    internal class BufferObject<TData> : GraphicsResource, IDisposable
        where TData : unmanaged
    {
        public uint NativePointer { get; private set; }
        public BufferTargetARB BufferType { get; private set; }

        private nuint _datumSize;

        public BufferObject(nuint datumUnitSize, BufferTargetARB bufferType)
        {
            _datumSize = datumUnitSize;
            BufferType = bufferType;
        }

        public void SetData(ref TData[] data)
            => SetData(data.AsSpan());

        public void SetData(Span<TData> data)
            => SetData(data, BufferUsageARB.StaticDraw);

        public void SetData(Span<TData> data, BufferUsageARB usage)
        {
            if (GraphicsDevice == null)
            {
                throw new NullReferenceException("VBO not bound!");
            }
            var gl = GraphicsDevice.GetOpenGLInstance();

            gl.BindBuffer(BufferType, NativePointer);
            gl.BufferData<TData>(BufferType, _datumSize * (nuint)data.Length, data, usage);
        }

        public unsafe void SetData(void* data, nuint length)
            => SetData(data, length, BufferUsageARB.StaticDraw);

        public unsafe void SetData(void* data, nuint length, BufferUsageARB usage)
        {
            if (GraphicsDevice == null)
            {
                throw new NullReferenceException("VBO not bound!");
            }
            var gl = GraphicsDevice.GetOpenGLInstance();

            gl.BindBuffer(BufferType, NativePointer);
            gl.BufferData(BufferType, length * _datumSize, data, usage);
        }

        protected override void DeviceBound(GraphicsDevice device)
        {
            var gl = device.GetOpenGLInstance();
            NativePointer = gl.GenBuffer();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {

            }
        }
    }
}
