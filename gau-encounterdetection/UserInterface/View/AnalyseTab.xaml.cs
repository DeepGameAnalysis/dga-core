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
using EDGui.ViewModel;

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
            DataContext = new AnalyseTabModel();
        }



        private void InteractiveMapClicked(object sender, MouseButtonEventArgs e)
        {
            if(e.Source is PlayerShape)
            {
                PlayerShape s = e.Source as PlayerShape;
                Console.WriteLine("Clicked playershape at: "+ s.X +" "+s.Y);
            }
        }
    }
}
