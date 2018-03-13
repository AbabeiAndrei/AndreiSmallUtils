using System;

namespace AndreiSmallUtils.WinOnTop.KeyModel
{
    public delegate void KeyPressHandler(IInterpretor sender, IKeyPressArgs args);

    public interface IKeyPressArgs
    {
        ConsoleKeyInfo Key { get; }
    }

    public class KeyPressArgs : EventArgs, IKeyPressArgs
    {
        public ConsoleKeyInfo Key { get; }

        public KeyPressArgs(ConsoleKeyInfo key)
        {
            Key = key;
        }
    }
}
