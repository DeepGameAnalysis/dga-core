using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDGui.Model
{
    internal class Demo : INotifyPropertyChanged
    {
        public Demo()
        {
        }

        string _GameName;
        public string GameName
        {
            get
            {
                return _GameName;
            }
            set
            {
                if (_GameName != value)
                {
                    _GameName = value;
                    OnPropertyChanged("GameName");
                }
            }
        }

        string _DemofileName;
        public string DemofileName
        {
            get
            {
                return _DemofileName;
            }
            set
            {
                if (_DemofileName != value)
                {
                    _DemofileName = value;
                    OnPropertyChanged("DemofileName");
                }
            }
        }

        double _DemoSize;
        public double DemoSize
        {
            get
            {
                return _DemoSize;
            }
            set
            {
                if (_DemoSize != value)
                {
                    _DemoSize = value;
                    OnPropertyChanged("DemoSize");
                }
            }
        }

        public string FileAccessed { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
