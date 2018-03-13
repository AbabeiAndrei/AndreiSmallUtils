using System;

namespace AndreiSmallUtils.Utils
{
    public static class StringEx
    {
        public static string ReplaceFirst(string str, string find, string replace = "")
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            if (string.IsNullOrEmpty(find))
                return str;

            var index = str.IndexOf(find, StringComparison.InvariantCulture);

            return index >= 0 
                        ? str.Remove(index, find.Length).Insert(index, replace)
                        : str;
        }


        public static string ReplaceLast(this string str, string find, string replace = "")
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            if (string.IsNullOrEmpty(str))
                return str;

            var index = str.LastIndexOf(find, StringComparison.InvariantCulture);

            return index >= 0 
                        ? str.Remove(index, find.Length).Insert(index, replace) 
                        : str;
        }
    }
}
