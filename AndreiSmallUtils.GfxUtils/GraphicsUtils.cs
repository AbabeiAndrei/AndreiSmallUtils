using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndreiSmallUtils.GfxUtils
{
    public static class GraphicsUtils
    {
        public static bool IsBlack(this Color color)
        {
            return color.R == 0 && color.G == 0 && color.B == 0;
        }
    }
}
