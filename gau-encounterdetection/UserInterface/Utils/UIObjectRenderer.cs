using Clustering;
using EDGUI.Utils;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace gau_ed_gui.Utils
{
    class UIObjectRenderer
    {
        public Panel TargetUi;
        /*
        private void DrawPosition(Point2D position, Color color)
        {
            var ps = new System.Windows.Shapes.Ellipse();
            var vector = CoordinateConverter.GameToUIPosition(position);
            ps.Margin = new Thickness(vector.X, vector.Y, 0, 0);
            ps.Width = 2;
            ps.Height = 2;

            ps.Fill = new SolidColorBrush(color);
            ps.Stroke = new SolidColorBrush(color);
            ps.StrokeThickness = 0.5;

            TargetUi.Children.Add(ps);
        }

        private void DrawRect(Rectangle2D rect, Color color)
        {
            var ps = new System.Windows.Shapes.Rectangle();
            var vector = CoordinateConverter.GameToUIPosition(new Point2D(rect.X, rect.Y));
            ps.Margin = new Thickness(vector.X, vector.Y, 0, 0);
            ps.Width = rect.Width * (Math.Min(mappanel_width, mapdata_width) / Math.Max(mappanel_width, mapdata_width));
            ps.Height = rect.Height * (Math.Min(mappanel_height, mapdata_height) / Math.Max(mappanel_height, mapdata_height));

            ps.Fill = new SolidColorBrush(color);
            ps.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            ps.StrokeThickness = 0.5;

            TargetUi.Children.Add(ps);
        }

        private void DrawHollowRect(Rectangle2D rect, Color color)
        {
            var ps = new System.Windows.Shapes.Rectangle();
            var vector = CoordinateConverter.GameToUIPosition(new Point2D(rect.X, rect.Y));
            ps.Margin = new Thickness(vector.X, vector.Y, 0, 0);
            ps.Width = rect.Width * (Math.Min(mappanel_width, mapdata_width) / Math.Max(mappanel_width, mapdata_width));
            ps.Height = rect.Height * (Math.Min(mappanel_height, mapdata_height) / Math.Max(mappanel_height, mapdata_height));

            ps.Stroke = new SolidColorBrush(color);
            ps.StrokeThickness = 0.5;

            TargetUi.Children.Add(ps);
        }

        public void RenderHurtClusters(EventPositionCluster[] clusters, Hashtable hits)
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

        public void RenderHurtEvents()
        {
            Console.WriteLine("Render Hurtevents");
            _renderbw.DoWork += (sender, args) =>
            {
                foreach (var key in this.EncounterDetection.Data.Hit_hashtable.Keys)
                {
                    var vic = (Point2D)this.EncounterDetection.Data.Hit_hashtable[key];
                    var att = (Point2D)key;
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        drawPos(att, Color.FromRgb(255, 0, 0));
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

        public void RenderMapLevelClusters()
        {
            Console.WriteLine("Render Map Level Cluster");
            _renderbw.DoWork += (sender, args) =>
            {

                for (int i = 0; i < this.EncounterDetection.Data.Map.maplevels.Count(); i++)
                {
                    Console.WriteLine("Level: " + i);
                    Color color = Color.FromArgb(255, 0, 0, 0);
                    switch (i)
                    {
                        case 0:
                            color = Color.FromArgb(255, 255, 0, 0); break; //rot
                        case 1:
                            color = Color.FromArgb(255, 0, 255, 0); break; //grün
                        case 2:
                            color = Color.FromArgb(255, 0, 0, 255); break; //blau
                        case 3:
                            color = Color.FromArgb(255, 255, 255, 0); break; //gelb
                        case 4:
                            color = Color.FromArgb(255, 0, 255, 255); break; //türkis
                        case 5:
                            color = Color.FromArgb(255, 255, 0, 255); break; //lilarosa
                        case 6:
                            color = Color.FromArgb(255, 120, 0, 0); break; //lilarosa
                        case 7:
                            color = Color.FromArgb(255, 0, 120, 0); break; //lilarosa
                        case 8:
                            color = Color.FromArgb(255, 0, 120, 120); break; //lilarosa
                        case 9:
                            color = Color.FromArgb(255, 120, 0, 120); break; //lilarosa
                        case 10:
                            color = Color.FromArgb(255, 120, 120, 0); break; //lilarosa
                    }



                    foreach (var p in this.EncounterDetection.Data.Map.maplevels[i].Cluster.ClusterData)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            drawPos(p, color);
                        }));
                    }

                    Thread.Sleep(4000);
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        CaptureScreenshot();
                    }));
                    Thread.Sleep(4000);

                }
            };
            _renderbw.RunWorkerAsync();

            _renderbw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());
            };
        }

        public void renderMapLevels()
        {
            Console.WriteLine("Render Map Levels");
            _renderbw.DoWork += (sender, args) =>
            {

                for (int i = 0; i < this.EncounterDetection.Data.Map.maplevels.Count(); i++)
                {
                    Console.WriteLine("Level: " + i);
                    Color color = Color.FromArgb(255, 0, 0, 0);
                    switch (i)
                    {
                        case 0:
                            color = Color.FromArgb(255, 255, 0, 0); break; //rot
                        case 1:
                            color = Color.FromArgb(255, 0, 255, 0); break; //grün
                        case 2:
                            color = Color.FromArgb(255, 0, 0, 255); break; //blau
                        case 3:
                            color = Color.FromArgb(255, 255, 255, 0); break; //gelb
                        case 4:
                            color = Color.FromArgb(255, 0, 255, 255); break; //türkis
                        case 5:
                            color = Color.FromArgb(255, 255, 0, 255); break; //lilarosa
                        case 6:
                            color = Color.FromArgb(255, 120, 0, 0); break; //lilarosa
                        case 7:
                            color = Color.FromArgb(255, 0, 120, 0); break; //lilarosa
                        case 8:
                            color = Color.FromArgb(255, 0, 120, 120); break; //lilarosa
                        case 9:
                            color = Color.FromArgb(255, 120, 0, 120); break; //lilarosa
                        case 10:
                            color = Color.FromArgb(255, 120, 120, 0); break; //lilarosa
                    }


                    foreach (var r in this.EncounterDetection.Data.Map.maplevels[i].FreeCells.ToList())
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            if (r.Value.Blocked == true)
                                //drawRect(r.Value, color);
                                //else
                                drawRect(r.Value.Bounds, Color.FromRgb(255, 255, 255));

                        }));
                    }


                    Thread.Sleep(4000);
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        CaptureScreenshot();
                    }));
                    Thread.Sleep(4000);
                }
            };
            _renderbw.RunWorkerAsync();

            _renderbw.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    MessageBox.Show(args.Error.ToString());
            };
        }*/
    }
}
