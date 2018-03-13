using System;
using System.Collections.Generic;

namespace AndreiSmallUtils.WinOnTop
{
    public interface IInterpretor
    {
        IEnumerable<string> GetMenu();
        bool Response(ConsoleKeyInfo chr);
    }
}