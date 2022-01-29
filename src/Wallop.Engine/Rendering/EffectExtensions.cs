using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Rendering
{
    internal static class EffectExtensions
    {
        // TODO: Getters

        //=====================================================================
        // Uniform1
        //=====================================================================

        public static void SetUniform(this Effect instance, string name, bool value)
            => SetUniform(instance, name, value ? 1 : 0);

        public static void SetUniform(this Effect instance, string name, double value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform1(location, value);
        }

        public static void SetUniform(this Effect instance, string name, float value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform1(location, value);
        }

        public static void SetUniform(this Effect instance, string name, int value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform1(location, value);
        }

        public static void SetUniform(this Effect instance, string name, uint value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform1(location, value);
        }

        public static void SetUniform(this Effect instance, string name, ReadOnlySpan<double> value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform1(location, value);
        }

        public static void SetUniform(this Effect instance, string name, ReadOnlySpan<float> value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform1(location, value);
        }

        public static void SetUniform(this Effect instance, string name, ReadOnlySpan<int> value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform1(location, value);
        }

        public static void SetUniform(this Effect instance, string name, ReadOnlySpan<uint> value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform1(location, value);
        }


        //=====================================================================
        // Uniform2
        //=====================================================================


        public static void SetUniform(this Effect instance, string name, ref System.Numerics.Vector2 value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform2(location, ref value);
        }

        public static void SetUniform(this Effect instance, string name, System.Numerics.Vector2 value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform2(location, value);
        }

        public static void SetUniform(this Effect instance, string name, double x, double y)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform2(location, x, y);
        }

        public static void SetUniform(this Effect instance, string name, float v0, float v1)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform2(location, v0, v1);
        }

        public static void SetUniform(this Effect instance, string name, int v0, int v1)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform2(location, v0, v1);
        }

        public static void SetUniform(this Effect instance, string name, uint v0, uint v1)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform2(location, v0, v1);
        }


        //=====================================================================
        // Uniform3
        //=====================================================================


        public static void SetUniform(this Effect instance, string name, ref System.Numerics.Vector3 value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform3(location, ref value);
        }

        public static void SetUniform(this Effect instance, string name, System.Numerics.Vector3 value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform3(location, value);
        }

        public static void SetUniform(this Effect instance, string name, double x, double y, double z)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform3(location, x, y, z);
        }

        public static void SetUniform(this Effect instance, string name, float v0, float v1, float v2)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform3(location, v0, v1, v2);
        }

        public static void SetUniform(this Effect instance, string name, int v0, int v1, int v2)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform3(location, v0, v1, v2);
        }

        public static void SetUniform(this Effect instance, string name, uint v0, uint v1, uint v2)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform3(location, v0, v1, v2);
        }


        //=====================================================================
        // Uniform4
        //=====================================================================


        public static void SetUniform(this Effect instance, string name, System.Numerics.Quaternion value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform4(location, value);
        }

        public static void SetUniform(this Effect instance, string name, ref System.Numerics.Vector4 value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform4(location, ref value);
        }

        public static void SetUniform(this Effect instance, string name, System.Numerics.Vector4 value)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform4(location, value);
        }

        public static void SetUniform(this Effect instance, string name, double x, double y, double z, double w)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform4(location, x, y, z, w);
        }

        public static void SetUniform(this Effect instance, string name, float v0, float v1, float v2, float v3)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform4(location, v0, v1, v2, v3);
        }

        public static void SetUniform(this Effect instance, string name, int v0, int v1, int v2, int v3)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform4(location, v0, v1, v2, v3);
        }

        public static void SetUniform(this Effect instance, string name, uint v0, uint v1, uint v2, uint v3)
        {
            CheckNativePointer(instance);
            var gl = CheckEffectBound(instance);
            var location = GetUniformLocation(instance, gl, name);

            gl.Uniform4(location, v0, v1, v2, v3);
        }


        //=====================================================================
        // Utils
        //=====================================================================


        private static GL CheckEffectBound(Effect instance)
        {
            if (instance.GraphicsDevice == null)
            {
                throw new ArgumentNullException(nameof(GraphicsDevice), "GraphicsDevice not bound!");
            }
            return instance.GraphicsDevice.GetOpenGLInstance();
        }

        private static void CheckNativePointer(Effect instance)
        {
            if(instance.NativePointer == 0)
            {
                throw new ArgumentNullException(nameof(instance.NativePointer), "Shader program has no ID!");
            }
        }
        
        private static int GetUniformLocation(Effect instance, GL gl, string name)
        {
            var uniformLocation = gl.GetUniformLocation(instance.NativePointer, name);
            if(uniformLocation == -1)
            {
                throw new ArgumentNullException("Uniform not found!");
            }
            return uniformLocation;
        }
    }
}
