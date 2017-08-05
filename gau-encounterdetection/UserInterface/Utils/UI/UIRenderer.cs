using Clustering;
using Data.Gameobjects;
using Data.Gamestate;
using Data.Utils;
using Detection;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;
using MathNet.Spatial.Units;
using Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EDGui.Utils
{
    class GameRenderer
    {
        private const string META_PATH = @"D:\Ressources\CS GO Demofiles\CS GO Mapmetadata\";


        //
        // VISUALS OF THE GAME-REPRESENTATION DRAWN TO THE CANVAS
        //
        /// <summary>
        /// Entities on the field(RTS etc)
        /// </summary>
        private ObservableDictionary<long, EntityShape> Entities = new ObservableDictionary<long, EntityShape>();

        /// <summary>
        /// All links between players that are currently drawn
        /// </summary>
        private ObservableDictionary<long, LinkShape> Links = new ObservableDictionary<long, LinkShape>();

        /// <summary>
        /// Other important events displayed as a radial shape with radius of the event
        /// </summary>
        private ObservableDictionary<long, RadialEffectShape> ActiveEntities = new ObservableDictionary<long, RadialEffectShape>();

        /// <summary>
        /// 
        /// </summary>
        private ObservableDictionary<long, Shape> GameShapes = new ObservableDictionary<long, Shape>();

        /// <summary>
        /// Special Rendering Objects
        /// </summary>
        private ObservableCollection<Shape> SpecialRenderShapes = new ObservableCollection<Shape>();

        /// <summary>
        /// Origion of the current coordinatesystem used by the inputcoordinates
        /// </summary>
        private Point2D CoordOrigin;

        /// <summary>
        /// Dimension on which to render the coordinates
        /// </summary>
        private Vector2D Dimension;


        private string Mapname;
        private double mapimage_width;
        private double mapimage_height;
        private double scalefactor_map;

        public GameRenderer(MapMetaData mapmeta)
        {
            CoordOrigin = new Point2D(mapmeta.CoordOriginX, mapmeta.CoordOriginY);
            Dimension = new Vector2D(mapmeta.Width, mapmeta.Height);
            Mapname = mapmeta.Mapname;
        }

        long currentid;

        public void Test(Point2D vector)
        {
            Color color = Color.FromArgb(255, 255, 0, 0);
            EntityShape ps = new EntityShape()
            {
                Yaw = Angle.FromDegrees(-45).Radians,
                X = vector.X,
                Y = vector.Y,
                Radius = 4,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5,
                Active = true
            };
            SpecialRenderShapes.Add(ps);
        }

        public void DrawMapImage()
        {
            BitmapImage bi = new BitmapImage(new Uri(META_PATH + Mapname + "_radar.jpg", UriKind.Relative));
            //BitmapImage bi = new BitmapImage(new Uri(@"C:\Users\Patrick\LRZ Sync+Share\Bacheloarbeit\CS GO Encounter Detection\csgo-stats-ed\CSGO Analytics\CSGO Analytics\src\views\mapviews\" + mapname + "_radar.jpg", UriKind.Relative));


        }

        private float x, y;
        public void RenderReplayTick(Tick tick, CombatComponent comp)
        {
            currentid++;
            x += 0.1f;
            y += 0.1f;
            Test(new Point2D(x,y));
            //
            // Update map with all active components, player etc
            //
            foreach (var updatedPlayer in tick.GetUpdatedPlayers())
                if (Entities.ContainsKey(updatedPlayer.player_id))
                {
                    var shape = GameShapes[updatedPlayer.player_id] as EntityShape;
                    shape.UpdatePlayerShape(updatedPlayer);
                }
                else
                    DrawPlayer(updatedPlayer);


            if (comp != null && comp.links.Count != 0)
            {
                foreach (var link in comp.links)
                {
                    if (link.Collision != null) //Just draw collision point if given. No Link
                    {
                        DrawPosition(link.Collision.Value, UIColoring.GetEntityColor(link.GetActor()));
                        continue;
                    }
                    if (IsActiveLink(link, out long linkkey))
                    {
                        var shape = GameShapes[linkkey] as LinkShape;
                        shape.UpdateLinkShape(Entities[link.GetActor().player_id], Entities[link.GetReciever().player_id]);
                    }
                    // Old link -> update else draw new
                    else
                        DrawLink(link);
                }
            }
        }

        /// <summary>
        /// Check if a link is active and safe its linkkey for later access on the dictionary.
        /// </summary>
        /// <param name="link"></param>
        /// <param name="linkkey"></param>
        /// <returns></returns>
        private bool IsActiveLink(Link link, out long linkkey)
        {
            linkkey = LinkShape.HashLink(link);
            return Links.ContainsKey(linkkey);
        }

        private void DrawPlayer(Player player)
        {
            Color color = UIColoring.GetEntityColor(player);
            var ps = new EntityShape()
            {
                Yaw = Angle.FromDegrees(-player.Facing.Yaw).Radians,
                X = player.Position.X,
                Y = player.Position.Y,
                Radius = 4,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5,
                Active = true,
            };

            GameShapes[player.player_id] = ps;
        }

        private void DrawLink(Link link)
        {
            EntityShape aps = Entities[link.GetActor().player_id];
            EntityShape rps = Entities[link.GetReciever().player_id];
            LinkShape ls = new LinkShape()
            {
                X1 = aps.X,
                Y1 = aps.Y,
                X2 = rps.X,
                Y2 = rps.Y,
                StrokeThickness = 2,
                Stroke = UIColoring.GetLinkBrush(link.GetLinkType())
            };
            GameShapes.Add(LinkShape.HashLink(link), ls);
        }


        public void DrawPosition(Point2D position, Color color)
        {
            var ps = new System.Windows.Shapes.Ellipse()
            {
                Margin = new Thickness(position.X, position.Y, 0, 0),
                Width = 2,
                Height = 2,

                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5
            };
            SpecialRenderShapes.Add(ps);
        }

        public void DrawRect(Rectangle2D rect, Color color)
        {
            var ps = new System.Windows.Shapes.Rectangle()
            {
                Margin = new Thickness(rect.X, rect.Y, 0, 0),
                Width = rect.Width,
                Height = rect.Height,

                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                StrokeThickness = 0.5
            };
            SpecialRenderShapes.Add(ps);

        }

        public void DrawHollowRect(Rectangle2D rect, Color color)
        {
            var ps = new System.Windows.Shapes.Rectangle()
            {
                Margin = new Thickness(rect.X, rect.Y, 0, 0),
                Width = rect.Width,
                Height = rect.Height,

                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5
            };
            SpecialRenderShapes.Add(ps);
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
        /// Backgroundworker to realize parallel drawing and UI interaction
        /// </summary>
        private BackgroundWorker _renderbw = new BackgroundWorker();

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
