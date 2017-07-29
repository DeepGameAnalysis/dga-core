using Data.Gameobjects;
using EDGui.Utils;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EDGui.Elements
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UIInteractiveMap : UserControl
    {
        public UIInteractiveMap()
        {
            InitializeComponent();

            var vector = new Point2D(0, 200);
            Color color = Color.FromArgb(255, 255, 0, 0);

            PlayerShape ps = new PlayerShape()
            {
                Yaw = Angle.FromDegrees(-45).Radians,
                X = vector.X,
                Y = vector.Y,
                Radius = 4,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5,
                Active = true
            };

            MapCanvas.Children.Add(ps);
        }

        //
        // VARIABLES FOR GAMEVISUALS
        //
        private string MapName;
        private double MapScalefactor;
        private double MapImageWidth;
        private double MapImageHeight;


        //
        // UI VARIABLES
        //
        private float Tickrate;


        //
        // VISUALS OF THE GAME-REPRESENTATION
        //

        /// <summary>
        /// All players drawn on the minimap - adress players by key given through the game/steam etc
        /// </summary>
        private Dictionary<long, PlayerShape> PlayerShapes = new Dictionary<long, PlayerShape>();

        /// <summary>
        /// Entities on the field(RTS etc)
        /// </summary>
        private Dictionary<long, PlayerShape> Entities = new Dictionary<long, PlayerShape>();

        /// <summary>
        /// All links between players that are currently drawn
        /// </summary>
        private List<LinkShape> Links = new List<LinkShape>();

        /// <summary>
        /// Other important events displayed as a radial shape with radius of the event
        /// </summary>
        private List<RadialEffectShape> ActiveEntities = new List<RadialEffectShape>();


        private void DrawPlayer(Player player)
        {
            Color color = UIColoring.GetEntityColor(player);


            var vector = GamePositionToMapPosition(player.Position.SubstractZ(), new Point2D(), 200, 200);

            var ps = new PlayerShape()
            {
                Yaw = Angle.FromDegrees(-player.Facing.Yaw).Radians,
                X = vector.X,
                Y = vector.Y,
                Radius = 4,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5,
                Active = true,
            };


            PlayerShapes[player.player_id] = ps;
            AddChild(ps);
        }

        /// <summary>
        /// Project a point p with its coordinate origin and a given max dimension onto the MapCanvas
        /// </summary>
        /// <param name="p"></param>
        /// <param name="map_origin"></param>
        /// <param name="MapDataWidth"></param>
        /// <param name="MapDataHeight"></param>
        /// <returns></returns>
        public Point2D GamePositionToMapPosition(Point2D? p, Point2D map_origin, double MapDataWidth, double MapDataHeight)
        {
            // Calculate a given demo point into a point suitable for our gui minimap: therefore we need a rotation factor, the origin of the coordinate and other data about the map. 
            var x = Math.Abs(map_origin.X - p.Value.X) * (Math.Min(MapCanvas.Width, MapDataWidth) / Math.Max(MapCanvas.Width, MapDataWidth));
            var y = Math.Abs(map_origin.Y - p.Value.Y) * (Math.Min(MapCanvas.Height, MapDataHeight) / Math.Max(MapCanvas.Height, MapDataHeight));
            return new Point2D(x, y);
        }
    }
}
