using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndreiSmallUtils.Utils
{
    public static partial class WinApi
    {
        public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        public delegate void WinEventDelegate(IntPtr hWinEventHook, 
                                              uint eventType, 
                                              IntPtr hwnd, 
                                              int idObject, 
                                              int idChild, 
                                              uint dwEventThread, 
                                              uint dwmsEventTime);
    }
}
