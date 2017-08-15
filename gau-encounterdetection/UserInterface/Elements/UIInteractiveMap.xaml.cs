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
        public static readonly DependencyProperty Dependency = DependencyProperty.Register("MapName", typeof(string), typeof(UIInteractiveMap), new UIPropertyMetadata(MapNameChanged));
        public string MapName
        {
            get { return (string)GetValue(Dependency); }
            set { SetValue(Dependency, value); }
        }

        public static void MapNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((UIInteractiveMap)sender).MapLabel.Content += " "+e.NewValue.ToString();
        }

        public UIInteractiveMap()
        {
            InitializeComponent();
        }
    }
}
