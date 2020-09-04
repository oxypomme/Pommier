using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OxyUtils
{
    internal static class ScreenController
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private static string GetTitle(IntPtr handle)
        {
            string windowText = "Inconnu";
            StringBuilder Buff = new StringBuilder(256);
            if (GetWindowText(handle, Buff, 256) > 0)
            {
                windowText = Buff.ToString();
            }
            return windowText;
        }

        public static bool IsForegroundFullScreen() => IsForegroundFullScreen(Screen.PrimaryScreen);

        public static bool IsForegroundFullScreen(Screen screen)
        {
            RECT rect = new RECT();
            GetWindowRect(new HandleRef(null, GetForegroundWindow()), ref rect);
            return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top).Contains(screen.Bounds);
        }

        public static string GetForegroundName() => GetTitle(GetForegroundWindow());
    }
}