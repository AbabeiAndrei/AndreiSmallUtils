using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace AndreiSmallUtils.Utils.Models
{
    public class WindowInfo
    {
        public IntPtr Handle { get; set; }

        public string Title { get; set; }

        public bool TopMost { get; set; }

        public WindowInfo(IntPtr handle, string title)
        {
            Handle = handle;
            Title = title;
        }

        public override string ToString() 
        {
            return Title;
        }
    }
}
