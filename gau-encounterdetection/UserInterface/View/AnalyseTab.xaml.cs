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
using Shapes;
using MathNet.Spatial.Units;
using MathNet.Spatial.Euclidean;

namespace EDGui.Views
{
    /// <summary>
    /// Interaction logic for AnalyseTab.xaml
    /// </summary>
    public partial class AnalyseTab : UserControl
    {
        public AnalyseTab()
        {
            InitializeComponent();

            PlayerShape ps = new PlayerShape();
            ps.Yaw = Angle.FromDegrees(-78).Radians;
            var vector = new Point2D(0,200);
            ps.X = vector.X;
            ps.Y = vector.Y;
            ps.Radius = 4;
            Color color = Color.FromArgb(255, 255, 0, 0);
            ps.Fill = new SolidColorBrush(color);
            ps.Stroke = new SolidColorBrush(color);
            ps.StrokeThickness = 0.5;
            ps.Active = true;


            interactiveMap.Children.Add(ps);
        }



        private void InteractiveMapClicked(object sender, MouseButtonEventArgs e)
        {
            if(e.Source is PlayerShape)
            {
                PlayerShape s = e.Source as PlayerShape;
                Console.WriteLine("Clicked playershape at: "+ s.X +" "+s.Y);
            }
        }

        private void PlayClicked(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
