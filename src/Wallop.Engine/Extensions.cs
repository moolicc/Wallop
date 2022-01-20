using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Wallop.Engine
{
    internal static class Extensions
    {
        public static void WaitAndThrow(this Task task)
        {
            task.Wait();
            if(task.IsFaulted && task.Exception != null)
            {
                throw task.Exception;
            }
        }

        public static Vector2 TopLeft(this Rectangle instance)
            => new Vector2(instance.Left, instance.Top);

        public static Vector2 TopRight(this Rectangle instance)
            => new Vector2(instance.Right, instance.Top);

        public static Vector2 BottomRight(this Rectangle instance)
            => new Vector2(instance.Right, instance.Bottom);

        public static Vector2 BottomLeft(this Rectangle instance)
            => new Vector2(instance.Left, instance.Bottom);
    }
}
