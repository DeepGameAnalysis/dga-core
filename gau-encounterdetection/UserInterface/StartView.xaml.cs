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
using MahApps.Metro.Controls;
using Xceed.Wpf.Toolkit;

namespace EDGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartView
    {
        public StartView()
        {
            InitializeComponent();

        }

    }

    internal class DemoEntry
    {
        public DemoEntry()
        {
        }

        public string GameName { get; set; }
        public string DemofileName { get; set; }
        public int DemoSize { get; set; }
        public string FileAccessed { get; set; }
    }
}
