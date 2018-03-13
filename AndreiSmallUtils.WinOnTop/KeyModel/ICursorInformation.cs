using System;

namespace AndreiSmallUtils.WinOnTop.KeyModel
{
    public interface ICursorInformation
    {
        int? OldCursorPosition { get; }
        int? NewCursorPosition { get; }
        ConsoleKeyInfo Key { get; }
    }
}