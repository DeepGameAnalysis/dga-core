using Data.Gamestate;
using Detection;
using EDGui.Utils;
using GameStateGenerators;
using Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace EDGui.ViewModel
{
    class AnalyseTabModel : ViewModelBase
    {

        private const string META_PATH = @"D:\Ressources\CS GO Demofiles\CS GO Mapmetadata\";

        /// <summary>
        /// 
        /// </summary>
        private EncounterDetection EncounterDetection;

        /// <summary>
        /// The gamestate to apply the encounter detection on
        /// </summary>
        private ReplayGamestate Gamestate;

        /// <summary>
        /// The replay returned by the algorithm
        /// </summary>
        private EncounterDetectionReplay Replay;


        public RelayCommand DetectEncountersCommand { get; set; }
        public RelayCommand PlayReplayCommand { get; set; }
        public RelayCommand StopReplayCommand { get; set; }

        public AnalyseTabModel()
        {
            DetectEncountersCommand = new RelayCommand(DetectEncounters);
            PlayReplayCommand = new RelayCommand(PlayReplay);
        }

        /// <summary>
        /// Backgroundworker to handle the replay. Especially preventing UI-Thread Blocking!
        /// </summary>
        private BackgroundWorker _replaybw = new BackgroundWorker();
        private ManualResetEvent _busy = new ManualResetEvent(true);

        private void PlayReplay(object obj)
        {
            Console.WriteLine("Play Match");

            /*
            if (Replay == null)
                return;

            _replaybw.DoWork += (sender, args) =>
            {
                int last_tickid = 0;

                foreach (var tuple in Replay.getReplayData())
                {
                    if (_replaybw.IsBusy) // Give _busy a chance to reset backgroundworker
                        _busy.WaitOne();

                    Tick tick = tuple.Key; // Tick is containing the game data (player positions, values of health, facing etc)
                    CombatComponent comp = tuple.Value; //Comp is containing the encounter detection data (links)

                    if (last_tickid == 0)
                        last_tickid = tick.tick_id;
                    int dt = tick.tick_id - last_tickid;

                    int passedTime = (int)(dt * 1000 / Tickrate);// + 2000;

                    //Jump out of backgroundworker to update UI
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        RenderTick(tick, comp);
                    }));

                    Thread.Sleep(passedTime);

                    last_tickid = tick.tick_id;
                }
            };

            _replaybw.RunWorkerAsync();

            _replaybw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());
            };
            */
        }

        void DetectEncounters(object parameter)
        {
            if(parameter == null) return;
            EncounterDetection.DetectEncounters();
        }

    }
}
