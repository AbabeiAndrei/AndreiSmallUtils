using System;

namespace AndreiSmallUtils.WinOnTop.KeyModel
{
    public delegate bool CursorMoveHandler(IInterpretor sender, ICursorInformation args);

    public class CursorInformation : EventArgs, ICursorInformation
    {
        #region Implementation of ICursorInformation

        public int? OldCursorPosition { get; }
        public int? NewCursorPosition { get; }
        public ConsoleKeyInfo Key { get; }

        #endregion

        #region Constructors

        public CursorInformation(int? oldCursorPosition, int? newCursorPosition, ConsoleKeyInfo key)
        {
            OldCursorPosition = oldCursorPosition;
            NewCursorPosition = newCursorPosition;
            Key = key;
        }

        #endregion
    }
}
