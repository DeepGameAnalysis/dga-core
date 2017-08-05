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

        public ObservableCollection<EDTask> SelectedDemos { get; set; }
        public ObservableCollection<EDTask> ReadyDemos { get; set; }

        public RelayCommand DetectEncountersCommand { get; set; }
        public RelayCommand AddDemosCommand { get; set; }
        public RelayCommand AnalyseDemosCommand { get; set; }
        public RelayCommand AddAnalyseDemosCommand { get; set; }
        public RelayCommand AddExistingDemosCommand { get; set; }
        public RelayCommand OpenParsingOptionsCommand { get; set; }

        private OpenFileDialog _DemoDialog;
        private OpenFileDialog _JSONDialog;

        private static ParseTaskSettings ParseTask = new ParseTaskSettings
        {
            DestPath = OUT_PATH,
            ShowSteps = false,
            Specialevents = false,
            HighDetailPlayer = false,
            PositionUpdateInterval = 250,
            Settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.None }
        };

        private string CurrentParseTaskPath;

        public ManageDemosTabModel()
        {
            SelectedDemos = new ObservableCollection<EDTask>();
            ReadyDemos = new ObservableCollection<EDTask>();

            AddDemosCommand = new RelayCommand(AddNewDemo);
            AddAnalyseDemosCommand = new RelayCommand(AddAndAnalyseDemo);
            AnalyseDemosCommand = new RelayCommand(AnalyseExistingDemo);
            AddExistingDemosCommand = new RelayCommand(AddExistingDemo);
            OpenParsingOptionsCommand = new RelayCommand(OpenParsingOptions);

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
        }

        private void ParseNewDemo(object obj)
        {
            DemoDataIO.ParseDemoFile(CurrentParseTaskPath, ParseTask);
        }

        private void ParseAllNewDemos(object obj)
        {
            foreach(var path in SelectedDemos)
                DemoDataIO.ParseDemoFile(path.DemofileName, ParseTask);
        }

        private void OpenParsingOptions(object obj)
        {
            Console.WriteLine("Open parsing options");
            new ParsingSettingsView().Show();
        }

        private void AddNewDemo(object obj)
        {
            Nullable<bool> result = _DemoDialog.ShowDialog();
            if (result == true)
                foreach (string dem in _DemoDialog.FileNames)
                    if (System.IO.Path.GetExtension(dem) == ".dem")
                    {
                        var task = new EDTask()
                        {
                            DemofileName = System.IO.Path.GetFileName(dem),
                            DemoSize = new System.IO.FileInfo(dem).Length / 1000 / 1000
                        };

                        if (!SelectedDemos.Contains(task))
                            SelectedDemos.Add(task);
                    }
        }

        private void AddExistingDemo(object obj)
        {
            Nullable<bool> result = _JSONDialog.ShowDialog();
            if (result == true)
                foreach (string json in _JSONDialog.FileNames)
                    if (System.IO.Path.GetExtension(json) == ".json")
                    {
                        var task = new EDTask()
                        {
                            DemofileName = System.IO.Path.GetFileName(json),
                            DemoSize = new System.IO.FileInfo(json).Length / 1000 / 1000
                        };

                        if (!ReadyDemos.Contains(task))
                            ReadyDemos.Add(task);
                    }
        }

        private void AddAndAnalyseDemo(object obj)
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
                        var encounterdetection = DemoDataIO.ReadDemoJSON(dem.Replace(".dem",".json"), ParseTask);
                        ProvideReplay(encounterdetection);
                    }
            _DemoDialog.Multiselect = true;

        }

        private void AnalyseExistingDemo(object obj)
        {
            //Parse the chosen replay json to object and send it to the analysetab
            //ParseTask.SrcPath = CurrentParseTaskPath;
            //var replay = FileWorker.ReadDemoJSON(CurrentParseTaskPath, ParseTask);
            ProvideReplay(null);

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
