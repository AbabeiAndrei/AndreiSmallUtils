using System;
using System.Text;
using System.Collections.Generic;

using AndreiSmallUtils.Utils.Models;

namespace AndreiSmallUtils.Utils
{
    public static class WindowsUtils
    {
        public static IEnumerable<WindowInfo> GetWindows()
        {
            var shellWindow = WinApi.GetShellWindow();
            var windows = new List<WindowInfo>();

            WinApi.EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) 
                    return true;

                if (!WinApi.IsWindowVisible(hWnd)) 
                    return true;

                int length = WinApi.GetWindowTextLength(hWnd);

                if (length == 0) 
                    return true;

                var builder = new StringBuilder(length);
                WinApi.GetWindowText(hWnd, builder, length + 1);
                
                windows.Add(new WindowInfo(hWnd, builder.ToString())
                {
                    TopMost = WinApi.IsWindowTopMost(hWnd)
                });

                return true;

            }, 0);

            return windows;
        }

    }
}
