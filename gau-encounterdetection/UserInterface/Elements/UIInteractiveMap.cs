using Data.Gameobjects;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace EDGui.Elements
{
    /// <summary>
    /// Creates a map with a given picture and metadata where players are running around doing their stuff.
    /// Players can be clicked for more info. The coordinate system begins at the upper left corner at grows in x to the right and in y to the bottom
    /// </summary>
    class UIInteractiveMap : Canvas
    {
        Hashtable PlayerShapes;

        public UIInteractiveMap()
        {


        }

        private void DrawPlayer(Player p)
        {
            Color color;
            if (p.GetTeam() == Team.T)
                color = Color.FromArgb(255, 255, 0, 0);
            else
                color = Color.FromArgb(255, 0, 0, 255);
            var vector = GamePositionToMapPosition(p.Position.SubstractZ(),new Point2D(),200,200,200,200);

            var ps = new PlayerShape()
            {
                Yaw = Angle.FromDegrees(-p.Facing.Yaw).Radians,
                X = vector.X,
                Y = vector.Y,
                Radius = 4,


                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5,
                Active = true,
            };


            PlayerShapes[p.player_id] = ps;
            Children.Add(ps);
        }

        public Point2D GamePositionToMapPosition(Point2D? p, Point2D map_origin, double ParentWidth, double ParentHeight, double MapDataWidth, double MapDataHeight)
        {
            // Calculate a given demo point into a point suitable for our gui minimap: therefore we need a rotation factor, the origin of the coordinate and other data about the map. 
            var x = Math.Abs(map_origin.X - p.Value.X) * (Math.Min(ParentWidth, MapDataWidth) / Math.Max(ParentWidth, MapDataWidth));
            var y = Math.Abs(map_origin.Y - p.Value.Y) * (Math.Min(ParentHeight, MapDataHeight) / Math.Max(ParentHeight, MapDataHeight));
            return new Point2D(x, y);
        }
    }
}
