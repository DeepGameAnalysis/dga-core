using Data.Gamestate;
using Detection;
using EDGui.Utils;
using GameStateGenerators;
using Microsoft.Win32;
using Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDGui.ViewModel
{
    class ManageDemosTabModel : ViewModelBase
    {

        public RelayCommand DetectEncountersCommand { get; set; }
        public RelayCommand AddDemosCommand { get; set; }
        public RelayCommand AnalyseDemosCommand { get; set; }
        public RelayCommand AddExistingDemosCommand { get; set; }

        private OpenFileDialog _DemoDialog;
        private OpenFileDialog _JSONDialog;

        public ManageDemosTabModel()
        {
            AddDemosCommand = new RelayCommand(AddDemo);
            AnalyseDemosCommand = new RelayCommand(AnalyseDemo);
            AddExistingDemosCommand = new RelayCommand(AddExistingDemo);

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





        private void AddDemo(object obj)
        {
            Nullable<bool> result = _DemoDialog.ShowDialog();
            if (result == true)
            {
                foreach (string dem in _DemoDialog.FileNames)
                {

                }
            }
        }

        private void AnalyseDemo(object obj)
        {
            SwitchToTabAtIndex(0);
        }

        private void AddExistingDemo(object obj)
        {
            Nullable<bool> result = _JSONDialog.ShowDialog();
            if (result == true)
            {
                foreach (string dem in _JSONDialog.FileNames)
                {

                }
            }
        }

        private void SwitchToTabAtIndex(int index)
        {
            var startview = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is StartView) as StartView;
            Application.Current.Dispatcher.BeginInvoke((Action)(() => startview.MainTabControl.SelectedIndex = index));
        }
    }
}
