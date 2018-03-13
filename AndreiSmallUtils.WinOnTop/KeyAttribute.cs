using System;

namespace AndreiSmallUtils.WinOnTop
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class KeyAttribute : Attribute
    {
        #region Properties

        public ConsoleKey[] Chars { get; }

        public int Order { get; set; }

        public string Event { get; set; }

        #endregion

        #region Constructors

        public KeyAttribute(params ConsoleKey[] chars)
        {
            Chars = chars;
            Order = int.MaxValue;
        }

        #endregion
    }
}
