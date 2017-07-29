using Clustering;
using Data.Gameobjects;
using EDGui.Utils;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;
using System;
using System.Collections;
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

namespace EDGui.Utils
{
    class UIObjectRenderer
    {
        /// <summary>
        /// The UI element this renderer should render to
        /// </summary>
        public Panel TargetUi { get; set; }

        private BackgroundWorker _renderbw;
        private Point2D coordorigin;
        private Vector2D dimension;

        private void DrawPosition(Point2D position, Color color)
        {
            var vector = CoordinateConverter.GameToUIPosition(position, coordorigin, dimension);
            var ps = new System.Windows.Shapes.Ellipse()
            {
                Margin = new Thickness(vector.X, vector.Y, 0, 0),
                Width = 2,
                Height = 2,

                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5
            };
            TargetUi.Children.Add(ps);
        }

        private void DrawRect(Rectangle2D rect, Color color)
        {
            var vector = CoordinateConverter.GameToUIPosition(new Point2D(rect.X, rect.Y), coordorigin, dimension);
            var scaler = CoordinateConverter.GetScaleFactor(dimension);
            var ps = new System.Windows.Shapes.Rectangle()
            {
                Margin = new Thickness(vector.X, vector.Y, 0, 0),
                Width = rect.Width * scaler.X,
                Height = rect.Height * scaler.Y,

                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                StrokeThickness = 0.5
            };
            TargetUi.Children.Add(ps);
        }

        private void DrawHollowRect(Rectangle2D rect, Color color)
        {
            var vector = CoordinateConverter.GameToUIPosition(new Point2D(rect.X, rect.Y), coordorigin, dimension);
            var scaler = CoordinateConverter.GetScaleFactor(dimension);

            var ps = new System.Windows.Shapes.Rectangle()
            {
                Margin = new Thickness(vector.X, vector.Y, 0, 0),
                Width = rect.Width * scaler.X,
                Height = rect.Height * scaler.Y,

                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5
            };
            TargetUi.Children.Add(ps);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clusters"></param>
        /// <param name="hits"></param>
        public void RenderHurtClusters(PlayerHurtCluster[] clusters, Hashtable hits)
        {
            for (int i = 0; i < clusters.Length; i++)
            {
                //var r = this.EDAlgorithm.attacker_clusters[i].getBoundings();
                var cluster = clusters[i];
                var victimpositions = new List<Point2D>();
                foreach (var attackpos in cluster.ClusterData)
                {
                    var victimpos = (Point2D)hits[attackpos];
                    victimpositions.Add(victimpos);
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        DrawPosition(attackpos.SubstractZ(), Color.FromRgb(255, 0, 0));
                        DrawPosition(victimpos, Color.FromRgb(0, 255, 0));
                    }));
                }
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    //drawHollowRect(c.getBoundings(), Color.FromRgb(255, 0, 0));
                }));
                var vr = GridFunctions.GetPointCloudBoundings(victimpositions);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    DrawHollowRect(vr, Color.FromRgb(0, 255, 0));
                }));
                //Thread.Sleep(4000);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Hit_hashtable"></param>
        public void RenderHurtEvents(Hashtable Hit_hashtable)
        {
            Console.WriteLine("Render Hurtevents");
            _renderbw.DoWork += (sender, args) =>
            {
                foreach (var key in Hit_hashtable.Keys)
                {
                    var vic = (Point2D)Hit_hashtable[key];
                    var att = (Point2D)key;
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        DrawPosition(att, Color.FromRgb(255, 0, 0));
                        //drawPos(vic, Color.FromRgb(0, 255, 0));
                    }));
                }
            };
            _renderbw.RunWorkerAsync();

            _renderbw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public void RenderMapLevelClusters(MapLevel[] maplevels)
        {
            Console.WriteLine("Render Map Level Cluster");
            _renderbw.DoWork += (sender, args) =>
            {

                for (int i = 0; i < maplevels.Count(); i++)
                {
                    Color color = UIColoring.RandomColor();
                    foreach (var p in maplevels[i].Cluster.ClusterData)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            DrawPosition(p, color);
                        }));
                    }

                }
            };
            _renderbw.RunWorkerAsync();

            _renderbw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());
            };
        }

        public void RenderMapLevels(MapLevel[] maplevels)
        {
            Console.WriteLine("Render Map Levels");
            _renderbw.DoWork += (sender, args) =>
            {

                for (int i = 0; i < maplevels.Count(); i++)
                {
                    Console.WriteLine("Level: " + i);
                    Color color = UIColoring.RandomColor();


                    foreach (var walkablecells in maplevels[i].FreeCells.ToList())
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            if (walkablecells.Value.Blocked == true)
                                DrawRect(walkablecells.Value.Bounds, Color.FromRgb(255, 255, 255));
                        }));
                    }
                }
            };
            _renderbw.RunWorkerAsync();

            _renderbw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());
            };
        }
    }
}
