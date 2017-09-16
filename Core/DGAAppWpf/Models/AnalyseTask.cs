using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGA.Model
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

        string _Path;
        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                if (_Path != value)
                {
                    _Path = value;
                    OnPropertyChanged("Path");
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

        DateTime _CreationDate;
        public DateTime CreationDate
        {
            get
            {
                return _CreationDate;
            }
            set
            {
                if (_CreationDate != value)
                {
                    _CreationDate = value;
                    OnPropertyChanged("CreationDate");
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
