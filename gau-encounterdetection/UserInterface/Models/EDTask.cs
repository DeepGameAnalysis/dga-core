using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDGui.Model
{
    /// <summary>
    /// Encounter detection Task - parse a new demo or read an already parsed demo
    /// </summary>
    internal class AnalyseTask : INotifyPropertyChanged
    {
        public AnalyseTask()
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

        public override bool Equals(object obj)
        {
            AnalyseTask e = obj as AnalyseTask;
            return DemofileName.Equals(e.DemofileName);
        }

        public string FileAccessed { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
