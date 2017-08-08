using EDGui.ViewModel;
using Microsoft.Win32;
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

namespace EDGui.Views
{
    /// <summary>
    /// Interaction logic for ManageDemosTab.xaml
    /// </summary>
    public partial class ManageDemosTab : UserControl
    {
        public ManageDemosTab()
        {
            InitializeComponent();
            DataContext = new ManageDemosTabModel();
        }
    }
}
