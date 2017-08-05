using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EDGui.Utils
{
    public class DataContextChangeEventHelper
    {
        public DataContextChangeEventHelper(FrameworkElement frameworkElement)
        {
            FrameworkElement = frameworkElement;
            DataContext = FrameworkElement.DataContext;
        }

        public FrameworkElement FrameworkElement { get; private set; }

        private Object dataContext;

        public Object DataContext
        {
            get
            {
                return dataContext;
            }
            set
            {
                dataContext = value;
                if (DataContextChanged != null)
                {
                    DataContextChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler DataContextChanged;

        public void Bind()
        {
            var binding = new Binding("DataContext")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            FrameworkElement.SetBinding(TextBlock.DataContextProperty, binding);
        }

    }
}
