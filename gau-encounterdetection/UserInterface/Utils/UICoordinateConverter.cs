using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EDGui.Utils
{
    class CoordinateConverter
    {
        /// <summary>
        /// The ui element this converter is working with
        /// </summary>
        public static Panel Ui;

        /// <summary>
        /// Function getting a CS:GO Position fetched from a replay file which returns a coordinate for our UI
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Point2D GameToUIPosition(Point2D? p, Point2D coordinateorigin, Vector2D dimension)
        {
            if (Ui == null) throw new Exception("Unkown UI to map game position to. Please define Ui Variable");
            // Calculate a given demo point into a point suitable for our gui minimap: therefore we need a rotation factor, the origin of the coordinate and other data about the map. 
            var scaler = GetScaleFactor(dimension);
            var x = Math.Abs(coordinateorigin.X - p.Value.X) * scaler.X;
            var y = Math.Abs(coordinateorigin.Y - p.Value.Y) * scaler.Y;
            Ui = null;
            return new Point2D(x, y);

        }

        public static Vector2D GetScaleFactor(Vector2D dimension)
        {
            var sx = (Math.Min(Ui.Width, dimension.X) / Math.Max(Ui.Width, dimension.X));
            var sy = (Math.Min(Ui.Height, dimension.Y) / Math.Max(Ui.Height, dimension.Y));
            return new Vector2D(sx,sy);
        }
    }
}
