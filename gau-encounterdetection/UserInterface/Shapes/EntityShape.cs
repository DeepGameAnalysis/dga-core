using Data.Gameobjects;
using EDGui.Utils;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes
{
    public class EntityShape : GameShape
    {
        /// <summary>
        /// Line of sight length of a playershape
        /// </summary>
        private const int LOS_LENGTH = 40;

        public double Radius { get; set; }

        public double Yaw { get; set; }

        public string Playerlevel { get; set; }

        private Point aimPoint = new Point(0, 0);

        public Geometry Geometry
        {
            get
            {
                var aimX = (X + LOS_LENGTH * Math.Cos(Yaw)); // Aim vector from Yaw -> dont forget toRadian for this calc
                var aimY = (Y + LOS_LENGTH * Math.Sin(Yaw));

                aimPoint.X = aimX;
                aimPoint.Y = aimY;
                FormattedText text = new FormattedText("",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Tahoma"),
                        7,
                        Brushes.Black);
                Geometry textg = text.BuildGeometry(new Point(X - 6, Y));
                Geometry line = new LineGeometry(new Point(X, Y), aimPoint);
                Geometry e = new EllipseGeometry(new Point(X, Y), Radius, Radius);
                GeometryGroup combined = new GeometryGroup();
                combined.Children.Add(e);
                combined.Children.Add(line);
                combined.Children.Add(textg);
                return combined;
            }
        }


        /// <summary>
        /// Update the appearance of a playershape by the data provided by a player
        /// </summary>
        /// <param name="p"></param>
        public void UpdatePlayerShape(Player p)
        {
            EntityShape ps = this;
            if (p.HP <= 0)
                ps.Active = false;
            if (p.HP > 0)
                ps.Active = true;

            if (!ps.Active)
            {
                ps.Fill = new SolidColorBrush(UIColoring.DEAD_PLAYER);
                ps.Stroke = new SolidColorBrush(UIColoring.DEAD_PLAYER);
                return;
            }
            else
            {
                Color color; //TODO: Choose Color by game and team. at the moment just csgo
                if (p.GetTeam() == Team.T)
                    color = UIColoring.TEAM_1;
                else
                    color = UIColoring.TEAM_2;
                ps.Fill = new SolidColorBrush(color);
                ps.Stroke = new SolidColorBrush(color);
            }

            if (p.IsSpotted)
            {
                var colorps = ((SolidColorBrush)ps.Fill);
                colorps.Color = UIColoring.ChangeColorBrightness(colorps.Color, 0.2f);
                ps.Fill = colorps;
                
            }
            else if (!p.IsSpotted)
            {
                var colorps = ((SolidColorBrush)ps.Fill);
                colorps.Color = UIColoring.ChangeColorBrightness(colorps.Color, -0.2f);
                ps.Fill = colorps;
            }

            ps.X = p.Position.X;
            ps.Y = p.Position.Y;
            ps.Yaw = Angle.FromDegrees(-p.Facing.Yaw).Radians;

        }
    }
}
