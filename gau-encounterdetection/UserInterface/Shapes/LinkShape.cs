using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using Data.Gameobjects;
using Detection;

namespace Shapes
{
    public class LinkShape : GameShape
    {

        private Direction LinkDirection;

        public double Length { get; set; }

        public double X2 { get; set; }

        public double Y2 { get; set; }

        public LinkShape()
        {
        }

        public Geometry Geometry
        {
            get
            {
                Point start = new Point(X, Y);
                Point end = new Point(X2, Y2);
                Geometry line = new LineGeometry(start, end);

                /*StreamGeometry geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;

                //Arrow tops for links TODO
                 using (StreamGeometryContext ctx = geometry.Open())
                {
                    // Begin the triangle at the point specified.
                    if (linkdirection == Direction.DEFAULT)
                    {
                        ctx.BeginFigure(new Point(10, 100), true, true);

                    } else
                    {

                    }
                    ctx.BeginFigure(new Point(10, 100), true, true);
                    ctx.LineTo(new Point(100, 100), true, false);
                    ctx.LineTo(new Point(100, 50), true , false );
                }*/

                // Freeze the geometry for performance benefits.
                //geometry.Freeze();


                GeometryGroup combined = new GeometryGroup();
                //combined.Children.Add(geometry);
                combined.Children.Add(line);
                return combined;
            }
        }


        /// <summary>
        /// Updates the Link between two playershapes
        /// </summary>
        /// <param name="aps"></param>
        /// <param name="rps"></param>
        public void UpdateLinkShape(EntityShape aps, EntityShape rps)
        {
            X = aps.X;
            Y = aps.Y;
            X2 = rps.X;
            Y2 = rps.Y;
        }

        internal static long HashLink(Link link)
        {
            long id1 = link.GetActor().player_id;
            long id2 = link.GetReciever().player_id;
            //int result = (int)(id1 ^ (id1 >> 32));
            //result = 31 * result + (int)(id2 ^ (id2 >> 32));
            return Math.Abs(id1+id2 / 2);
        }
    }
}
