﻿using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using Shapes;

namespace DGA.Utils.Converters
{
    public class XCoordinateConverter : IValueConverter
    {
        public Point2D coordinateorigin { get; private set; }
        public Canvas TargetUi { get; private set; }
        public Vector2D dimension { get; private set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = (double)value;
            var nx = Math.Abs(coordinateorigin.X - x) * (Math.Min(TargetUi.Width, dimension.X) / Math.Max(TargetUi.Width, dimension.X));
            return nx;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class YCoordinateConverter : IValueConverter
    {
        public Point2D coordinateorigin { get; private set; }
        public Canvas TargetUi { get; private set; }
        public Vector2D dimension { get; private set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var y = (double)value;
            var ny = Math.Abs(coordinateorigin.Y - y) * (Math.Min(TargetUi.Height, dimension.Y) / Math.Max(TargetUi.Height, dimension.Y));
            return ny;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        /// <summary>
        /// Function getting a CS:GO Position fetched from a replay file which returns a coordinate for our UI
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point2D GameToUIPosition(Canvas TargetUi, Point2D? p, Point2D coordinateorigin, Vector2D dimension)
        {
            if (TargetUi == null) throw new Exception("Unkown UI to map game position to. Please define Ui Variable");
            // Calculate a given demo point into a point suitable for our gui minimap: therefore we need a rotation factor, the origin of the coordinate and other data about the map. 
            var scaler = GetScaleFactor(TargetUi, dimension);
            var x = Math.Abs(coordinateorigin.X - p.Value.X) * scaler.X;
            var y = Math.Abs(coordinateorigin.Y - p.Value.Y) * scaler.Y;
            return new Point2D(x, y);

        }

        public Vector2D GetScaleFactor(Canvas TargetUi,Vector2D dimension)
        {
            if (TargetUi == null) throw new Exception("Unkown UI to define scaling factor. Please define Ui Variable");
            var sx = (Math.Min(TargetUi.Width, dimension.X) / Math.Max(TargetUi.Width, dimension.X));
            var sy = (Math.Min(TargetUi.Height, dimension.Y) / Math.Max(TargetUi.Height, dimension.Y));
            return new Vector2D(sx, sy);
        }
    }
}
