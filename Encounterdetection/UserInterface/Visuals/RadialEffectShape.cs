using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace Shapes
{
    class RadialEffectShape : Shape
    {
        /// <summary>
        /// Radius of the effect area that has to be displayed
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Duration how long this effect is active until it deletes itself
        /// </summary>
        public float Duration { get; set; }

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        #region Properties
        private static FrameworkPropertyMetadata XMetadata =
                new FrameworkPropertyMetadata(
                    90.0,     // Default value
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,    // Property changed callback
                    null);   // Coerce value callback

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(RadialEffectShape), XMetadata);


        private static FrameworkPropertyMetadata YMetadata =
                new FrameworkPropertyMetadata(
                    0.0,     // Default value
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,    // Property changed callback
                    null);   // Coerce value callback

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(RadialEffectShape), YMetadata);

        #endregion

        protected override Geometry DefiningGeometry
        {
            get
            {
                EllipseGeometry effectCircle = new EllipseGeometry(new Point(X, Y), Radius, Radius);
                LineGeometry center = new LineGeometry(new Point(X, Y), new Point(X, Y));

                GeometryGroup combined = new GeometryGroup();
                combined.Children.Add(effectCircle);
                combined.Children.Add(center);
                return combined;
            }
        }
    }
}
