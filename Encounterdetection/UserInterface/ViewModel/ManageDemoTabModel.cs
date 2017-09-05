using EDApplication;
using Data.Gamestate;
using Detection;
using EDGui.Model;
using EDGui.Utils;
using GameStateGenerators;
using Microsoft.Win32;
using Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JSONParsing;
using Newtonsoft.Json;

namespace EDGui.ViewModel
{
    class ManageDemosTabModel : ViewModelBase
    {
        private const string META_PATH = @"D:\Ressources\CS GO Demofiles\CS GO Mapmetadata\";
        private const string OUT_PATH = @"D:\Ressources\ParsedDemos\";

        //
        // BINDED DATA IN THIS VIEW
        //

        /// <summary>
        /// Task that await parsing into a JSON file format for later analysis
        /// </summary>
        public ObservableCollection<AnalyseTask> NewDemosTaskList { get; set; }

        /// <summary>
        /// Already parsed demos for which a JSON file is existing for analysis
        /// </summary>
        public ObservableCollection<AnalyseTask> ReadyDemosTaskList { get; set; }


        /// <summary>
        /// The current selected task for a demo in the ready demos list
        /// </summary>
        private AnalyseTask _SelectedReadyDemo;
        public AnalyseTask SelectedReadyDemo
        {
            get { return _SelectedReadyDemo; }
            set { _SelectedReadyDemo = value; RaisePropertyChanged("SelectedReadyDemo"); }
        }

        /// <summary>
        /// The current selected task for a demo in the todo demos list
        /// </summary>
        private AnalyseTask _SelectedNewDemo;
        public AnalyseTask SelectedNewDemo
        {
            get { return _SelectedNewDemo; }
            set { _SelectedNewDemo = value; RaisePropertyChanged("SelectedNewDemo"); }
        }

        //
        // DIALOGS
        //
        private OpenFileDialog _DemoDialog;
        private OpenFileDialog _JSONDialog;
        private ParsingSettingsView _OpenParsingSettings;


        private static ParseTaskSettings ParseTask = new ParseTaskSettings
        {
            DestPath = OUT_PATH,
            ShowSteps = false,
            Specialevents = false,
            HighDetailPlayer = false,
            PositionUpdateInterval = 250,
            Settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.None }
        };

        public ManageDemosTabModel()
        {
            NewDemosTaskList = new ObservableCollection<AnalyseTask>();
            ReadyDemosTaskList = new ObservableCollection<AnalyseTask>();

            _DemoDialog = new OpenFileDialog()
            {
                Multiselect = true,
                FileName = "", // Default file name
                DefaultExt = ".dem", // Default file extension
                Filter = "CSGO Demofile (.dem)|*.dem" // Filter files by extension
            };

            _JSONDialog = new OpenFileDialog()
            {
                Multiselect = true,
                FileName = "", // Default file name
                DefaultExt = ".json", // Default file extension
                Filter = "CSGO Demo Gamestatefile (.json)|*.json" // Filter files by extension
            };

            _OpenParsingSettings = new ParsingSettingsView();
        }

        #region Commands for UI Binding

        /// <summary>
        /// This command is parsing the demo files to uniform JSON
        /// </summary>
        public RelayCommand LoadDemosCommand
        {
            get
            {
                return _LoadDemosCommand ?? (_LoadDemosCommand = new RelayCommand(Execute_LoadDemos, CanExecute_LoadDemos));
            }
        }

        private RelayCommand _LoadDemosCommand;


        /// <summary>
        /// 
        /// </summary>
        public RelayCommand AddTasksCommand
        {
            get
            {
                return _AddTasksCommand ?? (_AddTasksCommand = new RelayCommand(Execute_AddTask, CanExecute_AddTask));
            }
        }

        private RelayCommand _AddTasksCommand;


        /// <summary>
        /// 
        /// </summary>
        public RelayCommand OpenParsingSettingsCommand
        {
            get
            {
                return _OpenParsingSettingsCommand ?? (_OpenParsingSettingsCommand = new RelayCommand(Execute_OpenParsingSettings, CanExecute_OpenParsingSettings));
            }
        }

        private RelayCommand _OpenParsingSettingsCommand;


        /// <summary>
        /// 
        /// </summary>
        public RelayCommand LoadAndAnalyseCommand
        {
            get
            {
                return _LoadAndAnalyseCommand ?? (_LoadAndAnalyseCommand = new RelayCommand(Execute_LoadAndAnalyse, CanExecute_LoadAndAnalyse));
            }
        }

        private RelayCommand _LoadAndAnalyseCommand;

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand AnalyseCommand
        {
            get
            {
                return _AnalyseCommand ?? (_AnalyseCommand = new RelayCommand(Execute_Analyse, CanExecute_Analyse));
            }
        }

        private RelayCommand _AnalyseCommand;
        #endregion

        #region Functions executed by User Input Commands

        private void Execute_LoadDemos(object obj)
        {
            foreach (var path in NewDemosTaskList)
            {
                DemoDataIO.ParseDemoFile(path.DemofileName, ParseTask);
                AddNewReadyDemo(path.DemofileName);
            }
        }

        private void Execute_OpenParsingSettings(object obj)
        {
            if (_OpenParsingSettings == null)
                _OpenParsingSettings = new ParsingSettingsView();

            _OpenParsingSettings.Show();
        }

        private void Execute_AddTask(object obj)
        {
            var list = obj as ObservableCollection<AnalyseTask>;

            OpenFileDialog pf = null;
            if (list == NewDemosTaskList)
                pf = _DemoDialog;
            else if(list == ReadyDemosTaskList)
                pf = _JSONDialog;

            Nullable<bool> result = pf.ShowDialog();

            if (result == true)
                foreach (string dem in pf.FileNames)
                {
                    var task = new AnalyseTask()
                    {
                        DemofileName = System.IO.Path.GetFileName(dem),
                        Path = dem,
                        DemoSize = new System.IO.FileInfo(dem).Length / 1000 / 1000
                    };

                    if (!list.Contains(task))
                        list.Add(task);
                }
        }

        private void Execute_DeleteTask(object obj)
        {

        }

        private void Execute_Analyse(object obj)
        {
            if (obj == null) return;
            var task = obj as AnalyseTask;
            var encounterdetection = DemoDataIO.ReadDemoJSON(task.Path, ParseTask);
            ProvideReplay(encounterdetection);
        }

        private void Execute_LoadAndAnalyse(object obj)
        {
            _DemoDialog.Multiselect = false;
            Nullable<bool> result = _DemoDialog.ShowDialog();
            if (result == true)
                foreach (string dem in _DemoDialog.FileNames)
                    if (System.IO.Path.GetExtension(dem) == ".dem")
                    {
                        ParseTask.SrcPath = dem;
                        ParseTask.DestPath = dem;
                        DemoDataIO.ParseDemoFile(dem, ParseTask);
                        string jsonpath = dem.Replace(".dem", ".json");
                        var encounterdetection = DemoDataIO.ReadDemoJSON(jsonpath, ParseTask);
                        AddNewReadyDemo(jsonpath);
                        ProvideReplay(encounterdetection);
                    }
            _DemoDialog.Multiselect = true;
        }


        #endregion


        #region Execution validation for above functions
        private bool CanExecute_AddTask(object obj)
        {
            return true;
        }

        private bool CanExecute_LoadAndAnalyse(object obj)
        {
            return true;
        }

        private bool CanExecute_LoadDemos(object obj)
        {
            return true;
        }

        private bool CanExecute_OpenParsingSettings(object obj)
        {
            return true;
        }

        private bool CanExecute_Analyse(object obj)
        {
            return true;
        }

        #endregion


        //
        // HELPERS
        //
        private void AddNewReadyDemo(string json)
        {
            json = json.Replace(".dem", ".json");
            var task = new AnalyseTask()
            {
                DemofileName = System.IO.Path.GetFileName(json),
                Path = json,
                DemoSize = new System.IO.FileInfo(json).Length / 1000 / 1000
            };

            if (!ReadyDemosTaskList.Contains(task))
                ReadyDemosTaskList.Add(task);
        }

        private static void ProvideReplay(EncounterDetection encounterdetection)
        {
            //Set new Datacontext for AnalyseTab and switch to this tab
            var startview = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is StartView) as StartView;
            TabItem t = startview.MainTabControl.Items[1] as TabItem;
            Application.Current.Dispatcher.BeginInvoke((Action)(() => startview.MainTabControl.SelectedIndex = 1));
            Application.Current.Dispatcher.BeginInvoke((Action)(() => t.DataContext = new AnalyseTabModel(encounterdetection)));
        }
    }
}
