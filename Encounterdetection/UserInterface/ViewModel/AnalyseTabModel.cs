using Data.Gameobjects;
using Data.Gamestate;
using Data.Utils;
using Detection;
using EDGui.Utils;
using EDGui.Visuals;
using GameStateGenerators;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EDGui.ViewModel
{
    public class AnalyseTabModel : ViewModelBase
    {

        /// <summary>
        /// The replay returned by the algorithm.
        /// </summary>
        private EncounterDetectionReplay Replay;


        /// <summary>
        /// Renderer using a canvas to display the game replay
        /// </summary>
        public GameRenderer Renderer { get; set; }

        //
        // UI VARIABLES
        //
        #region Binded Variables
        private string _TickRate;
        public string TickRate
        {
            get { return _TickRate; }
            set { _TickRate = value; RaisePropertyChanged("TickRate"); }
        }

        private string _MapName;
        public string MapName
        {
            get { return _MapName; }
            set { _MapName = value; RaisePropertyChanged("MapName"); }
        }

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


        public AnalyseTabModel(EncounterDetection EncounterDetection)
        {
            Replay = EncounterDetection.DetectEncounters();
            MapName = EncounterDetection.Data.Mapmeta.Mapname;
            TickRate = EncounterDetection.Data.Tickrate.ToString();
            Renderer = new GameRenderer(EncounterDetection.Data.Mapmeta);
            //Renderer.DrawMapImage();
        }
        #endregion

        #region Commands for UI Binding

        /// <summary>
        /// 
        /// </summary>
        public RelayCommand PlayReplayCommand
        {
            get
            {
                return _PlayReplayCommand ?? (_PlayReplayCommand = new RelayCommand(Execute_PlayReplay, CanExecute_PlayReplay));
            }
        }

        private RelayCommand _PlayReplayCommand;


        /// <summary>
        /// 
        /// </summary>
        public RelayCommand StopReplayCommand
        {
            get
            {
                return _StopReplayCommand ?? (_StopReplayCommand = new RelayCommand(Execute_StopReplay, CanExecute_StopReplay));
            }
        }

        private RelayCommand _StopReplayCommand;


        #endregion

        #region Functions executed by User Input
        /// <summary>
        /// Backgroundworker to handle the replay. Especially preventing UI-Thread Blocking!
        /// </summary>
        private BackgroundWorker _Replaybw = new BackgroundWorker();

        /// <summary>
        /// Reset and Set the BackgroundWorker
        /// </summary>
        private ManualResetEvent _Busy = new ManualResetEvent(true);

        /// <summary>
        /// Indicating if the game is stopped
        /// </summary>
        private bool IsPaused = false;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void Execute_PlayReplay(object obj)
        {
            Console.WriteLine("Play Match");
            if (IsPaused)
            {
                _Busy.Set();
                IsPaused = false;
                return;
            }
            else
            {
                Console.WriteLine("Game already running");
            }

            if (Replay == null)
                return;

            _Replaybw.DoWork += (sender, args) =>
            {
                int last_tickid = 0;

                foreach (var tuple in Replay.GetReplayData())
                {
                    if (_Replaybw.IsBusy) // Give _busy a chance to reset backgroundworker
                        _Busy.WaitOne();

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
                        Renderer.RenderReplayTickCollection(tick, comp);
                    }));

                    //Wait to render next frame
                    Thread.Sleep(passedTime);

                    last_tickid = tick.tick_id;
                }
            };

            _Replaybw.RunWorkerAsync();

            _Replaybw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());
            };

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void Execute_StopReplay(object obj)
        {
            _Busy.Reset();
            IsPaused = true;
        }
        #endregion

        #region Execution validation for above functions

        private bool CanExecute_PlayReplay(object obj)
        {
            return true;
        }

        private bool CanExecute_StopReplay(object obj)
        {
            return true;
        }
        #endregion

    }
}
