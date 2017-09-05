using Data.Gameobjects;
using Data.Gamestate;
using Detection;
using EDGui.Utils;
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

namespace EDGui.Elements
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
                ((UIInteractiveMap)sender).TickRateLabel.Content += " "+e.NewValue.ToString();
                _TickRateSet = true;
            }
            else
            {
                throw new Exception("TickRate can only be set once in a session");
            }
        }
        #endregion

        /// <summary>
        /// Renderer using a canvas to display the game replay
        /// </summary>
        public GameRenderer Renderer { get; set; }

        public UIInteractiveMap()
        {
            //Renderer = new GameRenderer(EncounterDetection.Data.Mapmeta);
            InitializeComponent();
        }
    }
}
