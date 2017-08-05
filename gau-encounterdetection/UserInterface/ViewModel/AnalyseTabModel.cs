using Data.Gameobjects;
using Data.Gamestate;
using Detection;
using EDGui.Utils;
using GameStateGenerators;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace EDGui.ViewModel
{
    class AnalyseTabModel : ViewModelBase
    {

        /// <summary>
        /// The replay returned by the algorithm and passed by the constrcutor from managetab
        /// </summary>
        private EncounterDetection EncounterDetection;

        /// <summary>
        /// 
        /// </summary>
        private EncounterDetectionReplay Replay;


        //
        // UI VARIABLES
        //
        private long _Time;
        public long Time
        {
            get { return _Time; }
            set { _Time = value; RaisePropertyChanged("Time"); }
        }

        private int _TickID;
        public int TickID
        {
            get { return _TickID; }
            set { _TickID = value; RaisePropertyChanged("TickID"); }
        }


        /// <summary>
        /// Renderer using a canvas to display the game
        /// </summary>
        private GameRenderer Renderer; //https://stackoverflow.com/questions/39074981/can-wpf-canvas-children-bind-observablecollection-contain-viewmodel-for-differen

        //
        // Commands for this VM
        //
        public RelayCommand PlayReplayCommand { get; set; }
        public RelayCommand StopReplayCommand { get; set; }

        public AnalyseTabModel(EncounterDetection ed)
        {
            EncounterDetection = ed;
            Replay = EncounterDetection.DetectEncounters();
            PlayReplayCommand = new RelayCommand(PlayReplay);
            StopReplayCommand = new RelayCommand(StopReplay);
            Renderer = new GameRenderer(EncounterDetection.Data.Mapmeta);
        }



        /// <summary>
        /// Backgroundworker to handle the replay. Especially preventing UI-Thread Blocking!
        /// </summary>
        private BackgroundWorker _replaybw = new BackgroundWorker();
        private ManualResetEvent _busy = new ManualResetEvent(true);
        private bool IsPaused = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void PlayReplay(object obj)
        {
            Console.WriteLine("Play Match");
            if (IsPaused)
            {
                _busy.Set();
                IsPaused = false;
                return;
            }

            if (Replay == null)
                return;

            _replaybw.DoWork += (sender, args) =>
            {
                Renderer.DrawMapImage();

                int last_tickid = 0;

                foreach (var tuple in Replay.GetReplayData())
                {
                    if (_replaybw.IsBusy) // Give _busy a chance to reset backgroundworker
                        _busy.WaitOne();

                    Tick tick = tuple.Key; // Tick is containing the game data (player positions, values of health, facing etc)
                    CombatComponent comp = tuple.Value; //Comp is containing the encounter detection data (links)

                    TickID = tick.tick_id; //Update TickID property

                    if (last_tickid == 0)
                        last_tickid = tick.tick_id;
                    int dt = tick.tick_id - last_tickid;

                    int passedTime = (int)(dt * 1000 / Replay.Tickrate);

                    Time += passedTime; // Update Time property

                    //Jump out of backgroundworker to render a tick of the game
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        Renderer.RenderReplayTick(tick, comp);
                    }));

                    //Wait to render next frame
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

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void StopReplay(object obj)
        {
            _busy.Reset();
            IsPaused = true;
        }

    }
}
