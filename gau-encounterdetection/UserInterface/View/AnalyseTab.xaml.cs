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
using EDGui.Utils;

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
            this.SetBinding(BoundDataContextProperty, new Binding());
        }

        public static readonly DependencyProperty BoundDataContextProperty = DependencyProperty.Register("BoundDataContext", typeof(object), typeof(AnalyseTab), new PropertyMetadata(null, OnBoundDataContextChanged));

        private static void OnBoundDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Data Context is set when a parsed demo provides an replay through managetab
            if(e.NewValue != null && e.OldValue == null)
            {
                var control = d as AnalyseTab;
                control.deactivatedMsg.Visibility = Visibility.Hidden;
            }
        }
    }
}
