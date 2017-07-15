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

namespace EDGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartView : Window
    {
        public StartView()
        {
            InitializeComponent();
        }

        private void ManageDemosSelected(object sender, RoutedEventArgs e)
        {
            _mainFrame.NavigationService.Navigate(new Uri("Views/ManageDemosView.xaml", UriKind.Relative));
        }

        private void AnalyseDemosSelected(object sender, RoutedEventArgs e)
        {
            _mainFrame.NavigationService.Navigate(new Uri("Views/AnalyseDemosView.xaml", UriKind.Relative));
        }

        private void GenerateVideoSelected(object sender, RoutedEventArgs e)
        {
            _mainFrame.NavigationService.Navigate(new Uri("Views/AnalyseDemosView.xaml", UriKind.Relative));
        }

        private void ImpressumSelected(object sender, RoutedEventArgs e)
        {
            _mainFrame.NavigationService.Navigate(new Uri("Views/Impressum.xaml", UriKind.Relative));
        }
    }
}
