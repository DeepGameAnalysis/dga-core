using Clustering;
using Data.Gameobjects;
using Data.Gamestate;
using Data.Utils;
using Detection;
using DGA.ViewModel;
using gau_ed_gui.Properties;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;
using MathNet.Spatial.Units;
using Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DGA.Utils
{
    /// <summary>
    /// It is just a drawing library, you would still need to hit-test on your own.
    /// I would recommend creating a UserControl,
    /// - Drawing with WritableBitmapEx
    /// - Hit-testing directly in the code-behind of the UserControl. You can use DrawingVisual to do the hit-test.
    /// </summary>
    public class GameRenderer : ViewModelBase
    {
        private const string META_PATH = @"D:\Ressources\CS GO Demofiles\CS GO Mapmetadata\";

        private const int LOS_LENGTH = 40;

        private const int ENTITY_RADIUS = 4;

        //
        // VISUALS OF THE GAME-REPRESENTATION DRAWN TO THE CANVAS
        //

        /// <summary>
        /// Special Rendering Objects
        /// </summary>
        public ObservableCollection<GameShape> RenderedShapes { get; } = new ObservableCollection<GameShape>();


        /// <summary>
        /// 
        /// </summary>
        public WriteableBitmap GameImageSource { get; set; }


        /// <summary>
        /// Origion of the current coordinatesystem used by the inputcoordinates
        /// </summary>
        private Point2D CoordOrigin;

        /// <summary>
        /// Dimension on which to render the coordinates
        /// </summary>
        private Vector2D Dimension;

        private BitmapImage Background;

        public GameRenderer(MapMetaData meta)
        {
            Dimension = new Vector2D(meta.Width, meta.Height);
            CoordOrigin = new Point2D(meta.CoordOriginX, meta.CoordOriginY);
            SetMapImage(meta);
        }


        public void SetMapImage(MapMetaData meta)
        {
            Background = new BitmapImage(new Uri(META_PATH + meta.Mapname + "_radar.jpg", UriKind.Relative));
            GameImageSource = new WriteableBitmap(Background);
        }


        private void DrawEntity(Player entity)
        {
            Color color = UIColoring.GetEntityColor(entity);

            var UIpos = GameToUIPosition(GameImageSource, entity.Position.SubstractZ(), CoordOrigin, Dimension);
            var Yaw = Angle.FromDegrees(-entity.Facing.Yaw).Radians;

            var aimX = (int)(UIpos.X + LOS_LENGTH * Math.Cos(Yaw)); // Aim vector from Yaw -> dont forget toRadian for this calc
            var aimY = (int)(UIpos.Y + LOS_LENGTH * Math.Sin(Yaw));

            GameImageSource.FillEllipseCentered((int)UIpos.X, (int)UIpos.Y, ENTITY_RADIUS, ENTITY_RADIUS, color);
            GameImageSource.DrawLineAa((int)UIpos.X, (int)UIpos.Y, aimX, aimY, color);
            GameImageSource.AddDirtyRect(new Int32Rect
            {
                X = (int)(UIpos.X - ENTITY_RADIUS / 2),
                Y = (int)(UIpos.Y - ENTITY_RADIUS / 2),
                Width = ENTITY_RADIUS * 2,
                Height = ENTITY_RADIUS * 2
            });
        }


        private void DrawLink(Link link)
        {
            Color color = UIColoring.GetLinkColor(link.GetLinkType());

            var apos = link.GetActor().Position.SubstractZ();
            var rpos = link.GetReciever().Position.SubstractZ();

            var aUIpos = GameToUIPosition(GameImageSource, apos, CoordOrigin, Dimension);
            var rUIpos = GameToUIPosition(GameImageSource, rpos, CoordOrigin, Dimension);

            GameImageSource.DrawLineAa((int)aUIpos.X, (int)aUIpos.Y, (int)rUIpos.X, (int)rUIpos.Y, color);
            GameImageSource.AddDirtyRect(new Int32Rect
            {
                X = (int)aUIpos.X,
                Y = (int)aUIpos.Y,
                Width = Math.Abs((int)aUIpos.X - (int)rUIpos.X),
                Height = Math.Abs((int)aUIpos.Y - (int)rUIpos.Y)
            });

        }



        internal void RenderReplayTick(Tick tick, CombatComponent comp)
        {
            // Reserve the back buffer for updates.
            GameImageSource.Lock();
            unsafe
            {
                int pBackBuffer = (int)GameImageSource.BackBuffer;
                //
                // Update map with all active components, players etc
                //
                using (GameImageSource.GetBitmapContext())
                {

                    foreach (var updatedPlayer in tick.GetUpdatedPlayers())
                        DrawEntity(updatedPlayer);


                    if (comp != null && comp.links.Count != 0)
                        foreach (var link in comp.links)
                            DrawLink(link);

                }
            }
            // Release the back buffer and make it available for display.
            GameImageSource.Unlock();
        }

        // The DrawPixel method updates the WriteableBitmap by using
        // unsafe code to write a pixel into the back buffer.
        private void DrawPixel(Point2D p)
        {
            int column = (int)p.X;
            int row = (int)p.Y;

            // Reserve the back buffer for updates.
            GameImageSource.Lock();

            unsafe
            {
                // Get a pointer to the back buffer.
                int pBackBuffer = (int)GameImageSource.BackBuffer;

                // Find the address of the pixel to draw.
                pBackBuffer += row * GameImageSource.BackBufferStride;
                pBackBuffer += column * 4;

                // Compute the pixel's color.
                int color_data = 255 << 16; // R
                color_data |= 128 << 8;   // G
                color_data |= 255 << 0;   // B

                // Assign the color data to the pixel.
                *((int*)pBackBuffer) = color_data;
            }

            // Specify the area of the bitmap that changed.
            GameImageSource.AddDirtyRect(new Int32Rect(column, row, 1, 1));

            // Release the back buffer and make it available for display.
            GameImageSource.Unlock();
        }


        /// <summary>
        /// Function getting a game(demo) position fetched from a replay file and returns a coordinate for our UI
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Point2D GameToUIPosition(WriteableBitmap rendertarget, Point2D? p, Point2D coordinateorigin, Vector2D dimension)
        {
            if (rendertarget == null) throw new Exception("Unkown UI to map game position to. Please define Ui Variable");
            // Calculate a given demo point into a point suitable for our gui minimap: therefore we need a rotation factor, the origin of the coordinate and other data about the map. 
            var scaler = GetScaleFactor(rendertarget, dimension);
            var x = Math.Abs(coordinateorigin.X - p.Value.X) * scaler.X;
            var y = Math.Abs(coordinateorigin.Y - p.Value.Y) * scaler.Y;
            return new Point2D(x, y);

        }

        /// <summary>
        /// Calculate the scale factor for a given render target with the dimension of the gamefield
        /// </summary>
        /// <param name="rendertarget"></param>
        /// <param name="dimension"></param>
        /// <returns></returns>
        private Vector2D GetScaleFactor(WriteableBitmap rendertarget, Vector2D dimension)
        {
            if (rendertarget == null) throw new Exception("Unkown UI to define scaling factor. Please define Ui Variable");
            var sx = (Math.Min(rendertarget.Width, dimension.X) / Math.Max(rendertarget.Width, dimension.X));
            var sy = (Math.Min(rendertarget.Height, dimension.Y) / Math.Max(rendertarget.Height, dimension.Y));
            return new Vector2D(sx, sy);
        }









        /*
        private void DrawPlayer(Player player)
        {
            Color color = UIColoring.GetEntityColor(player);
            var ps = new EntityShape()
            {
                Yaw = Angle.FromDegrees(-player.Facing.Yaw).Radians,
                X = GameToUIPosition(GameImageSource, player.Position.SubstractZ(), CoordOrigin, Dimension).X,
                Y = GameToUIPosition(GameImageSource, player.Position.SubstractZ(), CoordOrigin, Dimension).Y,
                Radius = 4,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5,
                Active = true,
            };
            GameImageSource.DrawEllipseCentered((int)ps.X, (int)ps.Y, 4, 4, color);
            //GameShapes[player.player_id] = ps;
            //SpecialRenderShapes.Add(ps);
        }

        private void DrawLink(Link link)
        {
            EntityShape aps = Entities[link.GetActor().player_id];
            EntityShape rps = Entities[link.GetReciever().player_id];
            LinkShape ls = new LinkShape()
            {
                X = Math.Abs(aps.X / 10),
                Y = Math.Abs(aps.Y / 10),
                X2 = Math.Abs(rps.X / 10),
                Y2 = Math.Abs(rps.Y / 10),
                StrokeThickness = 2,
                Stroke = UIColoring.GetLinkBrush(link.GetLinkType())
            };
            //GameShapes.Add(LinkShape.HashLink(link), ls);
            RenderedShapes.Add(ls);
        }


        public void DrawPosition(Point2D position, Color color)
        {
            var ps = new Ellipse()
            {
                Margin = new Thickness(position.X, position.Y, 0, 0),
                Width = 2,
                Height = 2,

                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5
            };
            //SpecialRenderShapes.Add(ps);
        }

        public void DrawRect(Rectangle2D rect, Color color)
        {
            var ps = new Rectangle()
            {
                Margin = new Thickness(rect.X, rect.Y, 0, 0),
                Width = rect.Width,
                Height = rect.Height,

                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                StrokeThickness = 0.5
            };
            //SpecialRenderShapes.Add(ps);

        }

        public void DrawHollowRect(Rectangle2D rect, Color color)
        {
            var ps = new Rectangle()
            {
                Margin = new Thickness(rect.X, rect.Y, 0, 0),
                Width = rect.Width,
                Height = rect.Height,

                Stroke = new SolidColorBrush(color),
                StrokeThickness = 0.5
            };
            //SpecialRenderShapes.Add(ps);
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
        public ObservableDictionary<long, GameShape> GameShapes { get; } = new ObservableDictionary<long, GameShape>();

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr dest, IntPtr source, int Length);

        private System.Drawing.Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new System.Drawing.Bitmap(bitmap);
            }
        }

        public void RenderReplayTick(Tick tick, CombatComponent comp)
        {

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
                    else
                        DrawLink(link); // Old link -> update else draw new
                }
            }

        }

        public void RenderReplayTickCollection(Tick tick, CombatComponent comp)
        {
            //
            // Update map with all active components, player etc
            //
            foreach (var updatedPlayer in tick.GetUpdatedPlayers())
                DrawPlayer(updatedPlayer);


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
        */
    }
}
