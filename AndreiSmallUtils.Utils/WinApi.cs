using System;
using System.Text;
using System.Runtime.InteropServices;

namespace AndreiSmallUtils.Utils
{
    public static partial class WinApi
    {
        private const string USER32_DLL = "user32.dll";

        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOPMOST = 0x0008;

        public const uint WINEVENT_OUTOFCONTEXT = 0;
        public const uint EVENT_SYSTEM_FOREGROUND = 3;
        
        [DllImport(USER32_DLL, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport(USER32_DLL, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport(USER32_DLL)]
        public static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport(USER32_DLL)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport(USER32_DLL)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport(USER32_DLL)]
        public static extern IntPtr GetShellWindow();
        
        [DllImport(USER32_DLL, SetLastError=true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport (USER32_DLL)]
        public static extern int SetWindowLong ( IntPtr hWnd, int nIndex, uint dwNewLong );

        public static bool IsWindowTopMost(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);

            return (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;
        }
        
        [DllImport(USER32_DLL)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(USER32_DLL)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        

        [DllImport(USER32_DLL)]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    }
}
