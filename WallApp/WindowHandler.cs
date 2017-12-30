using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//https://www.codeproject.com/articles/856020/draw-behind-desktop-icons-in-windows
namespace WallApp
{
    class WindowHandler
    {
        public static void SetParet(IntPtr childHandle)
        {
            IntPtr parentHandle = CreateWorkerW();
            Win32.SetParent(childHandle, parentHandle);
        }

        public static void RepaintDesktop()
        {
            IntPtr desktopHnd = Win32.GetDesktopWindow();
            Win32.SendMessage(desktopHnd, 0x000F, IntPtr.Zero, IntPtr.Zero);
        }

        private static IntPtr CreateWorkerW()
        {
            IntPtr progmanHWnd = FindProgman();
            IntPtr result = IntPtr.Zero;

            Win32.SendMessageTimeout(progmanHWnd, 0x052C, IntPtr.Zero, IntPtr.Zero,
                Win32.SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out result);

            IntPtr workerHandle = IntPtr.Zero;

            Win32.EnumWindows(new Win32.EnumWindowsProc((topHandle, topParamHandle) =>
            {
                IntPtr p = Win32.FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    workerHandle = Win32.FindWindowEx(IntPtr.Zero, topHandle, "WorkerW", IntPtr.Zero);
                }
                return true;
            }), IntPtr.Zero);

            return workerHandle;
        }

        private static IntPtr FindProgman()
        {
            IntPtr hWnd = Win32.FindWindow("Progman", null);
            if (hWnd == IntPtr.Zero)
            {
                throw new Exception();
            }
            return hWnd;
        }
    }
}
