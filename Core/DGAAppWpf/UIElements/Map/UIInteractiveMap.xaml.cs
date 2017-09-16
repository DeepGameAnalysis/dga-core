using Data.Gameobjects;
using Data.Gamestate;
using Detection;
using DGA.Utils;
using MathNet.Spatial;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace DGA.UIElements.Map
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UIInteractiveMap : UserControl
    {
        #region MapName Property
        public static readonly DependencyProperty MapNameDependency = DependencyProperty.Register("MapName", typeof(string), typeof(UIInteractiveMap), new UIPropertyMetadata(MapNameChanged));
        private static bool MapNameSet = false;
        public string MapName
        {
            get { return (string)GetValue(MapNameDependency); }
            set { SetValue(MapNameDependency, value); }
        }

        public static void MapNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!MapNameSet)
            {
                ((UIInteractiveMap)sender).MapLabel.Content += " " + e.NewValue.ToString();
                MapNameSet = true;
            }
            else
            {
                throw new Exception("MapName can only be set once in a session");
            }
        }
        #endregion

        #region TickRate Property
        public static readonly DependencyProperty TickRateDependency = DependencyProperty.Register("TickRate", typeof(string), typeof(UIInteractiveMap), new UIPropertyMetadata(TickRateChanged));
        private static bool _TickRateSet;
        public string TickRate
        {
            get { return (string)GetValue(TickRateDependency); }
            set { SetValue(TickRateDependency, value); }
        }

        public static void TickRateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_TickRateSet)
            {
                ((UIInteractiveMap)sender).TickRateLabel.Content += " " + e.NewValue.ToString();
                _TickRateSet = true;
            }
            else
            {
                throw new Exception("TickRate can only be set once in a session");
            }
        }
        #endregion

        #region Renderer Property
        /// <summary>
        /// Renderer using a canvas to display the game replay
        /// </summary>
        public static readonly DependencyProperty RendererDependency = DependencyProperty.Register("Renderer", typeof(GameRenderer), typeof(UIInteractiveMap), new UIPropertyMetadata(RendererChanged));
        private static bool _RendererSet;
        public GameRenderer Renderer
        {
            get { return (GameRenderer)GetValue(RendererDependency); }
            set { SetValue(RendererDependency, value); }
        }

        public static void RendererChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_RendererSet)
            {
                _RendererSet = true;
            }
            else
            {
                throw new Exception("Renderer can only be set once in a session");
            }
        }
        #endregion

        #region Zoom Property
        /// <summary>
        /// Renderer using a canvas to display the game replay
        /// </summary>
        public static readonly DependencyProperty ZoomFactorDependency = DependencyProperty.Register("ZoomFactor", typeof(double), typeof(UIInteractiveMap), new UIPropertyMetadata(_ZoomFactorChanged));
        public double ZoomFactor
        {
            get { return (double)GetValue(ZoomFactorDependency); }
            set { SetValue(ZoomFactorDependency, value); }
        }

        public static void _ZoomFactorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((UIInteractiveMap)sender).ZoomLabel.Content = "Zoom: " + e.NewValue.ToString();
        }

        #endregion

        private const double ZOOM_MAX_CAP = 4;

        private const double ZOOM_MIN_CAP = 0.5;

        private double MapWidth;

        private double MapHeight;

        private bool firstZoom = true;

        public UIInteractiveMap()
        {
            //Renderer = new GameRenderer(EncounterDetection.Data.Mapmeta);
            InitializeComponent();
            ZoomFactor = 1.0;
        }

        /// <summary>
        /// Handle a mouse click on the map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapClicked(object sender, MouseButtonEventArgs e)
        {
            var mapelement = (IInputElement)sender;
            Point mouseclickpoint = e.GetPosition((mapelement));
            Console.WriteLine(mouseclickpoint.ToString());
        }

        /// <summary>
        /// Handle a mouse scroll on the map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseScroll(object sender, MouseWheelEventArgs e)
        {
            var mapelement = (IInputElement)sender;
            Point mouseclickpoint = e.GetPosition((mapelement));
            ZoomMap(e.Delta, mouseclickpoint);// Always -120 or 120?!
        }

        /// <summary>
        /// Zooms the map into/from the center (point of mouse)
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="center"></param>
        private void ZoomMap(int delta, Point center)
        {
            if (firstZoom)
            {
                MapWidth = Map.Source.Width;
                MapHeight = Map.Source.Height;
                firstZoom = false;
            }

            var zoomfactor = ZoomFactor + delta / 120.0 / 10.0;
            ZoomFactor = MathExtended.Clamp(zoomfactor, ZOOM_MIN_CAP, ZOOM_MAX_CAP);
            Map.Width = MapWidth * ZoomFactor;
            Map.Height = MapHeight * ZoomFactor;
        }
    }
}
