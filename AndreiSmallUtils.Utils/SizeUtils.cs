using System.IO;
using System.Linq;

namespace AndreiSmallUtils.Utils
{
    public static class SizeUtils
    {
        private static readonly string[] _units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string ToFileSize(this long size, int unit = 0)
        {
            double dsize = size;
            while(dsize >= 1024) 
            {
                dsize /= 1024;
                ++unit;
            }

            return string.Format("{0:0.##} {1}", dsize, _units[unit]);
        }

        public static long DirectorySize(string path)
        {
            return DirectorySize(new DirectoryInfo(path));
        }

        public static long DirectorySize(DirectoryInfo directoryInfo) 
        {
            return  directoryInfo.GetFiles().Select(fi => fi.Length)
                                 .DefaultIfEmpty(0)
                                 .Sum() + 
                    directoryInfo.GetDirectories().Select(DirectorySize)
                                 .DefaultIfEmpty(0)
                                 .Sum();  
        }
    }
}
