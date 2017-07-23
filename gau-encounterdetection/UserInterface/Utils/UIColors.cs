using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EDGUI.Utils
{
    public class UIColors
    {
        public static Color DEAD_PLAYER = Color.FromArgb(255, 255, 255, 255);
        public static Color TEAM_1 = Color.FromArgb(255, 255, 0, 0);
        public static Color TEAM_2 = Color.FromArgb(255, 0, 0, 255);

        /// <summary>
        /// Return a lighter color tone of the given color
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color Lighten(Color c)
        {
            return c; //TODO
        }

        /// <summary>
        /// Return a darker color tone of the given color
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color Darken(Color c)
        {
            return c;
        }
    }
}
