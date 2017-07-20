using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EDGUI.Utils
{
    class CoordinateConverter
    {
        public static Panel Ui;
        /// <summary>
        /// Function getting a CS:GO Position fetched from a replay file which returns a coordinate for our UI
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Point2D GameToUIPosition(Point2D? p, Point2D coordinateorigin, Vector2D mapdimension)
        {
            if (Ui == null) throw new Exception("Unkown UI to map game position to. Please define Ui Variable");
            // Calculate a given demo point into a point suitable for our gui minimap: therefore we need a rotation factor, the origin of the coordinate and other data about the map. 
            var x = Math.Abs(coordinateorigin.X - p.Value.X) * (Math.Min(Ui.Width, mapdimension.X) / Math.Max(Ui.Width, mapdimension.X));
            var y = Math.Abs(coordinateorigin.Y - p.Value.Y) * (Math.Min(Ui.Height, mapdimension.Y) / Math.Max(Ui.Height, mapdimension.Y));
            Ui = null;
            return new Point2D(x, y);

        }
    }
}
