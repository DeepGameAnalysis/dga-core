using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows;
using System.Collections;
using QuadTree;
using QuadTree.Common;
using KDTree.Math;
using Data.Gameobjects;
using MathNet.Spatial.Euclidean;
using Clustering;

namespace Data.Utils
{
    /// <summary>
    /// Rebuild the map by use of positional data.
    /// The simple map constructor is creating a map grid and deciding which cell is a wall and which is not by all positions walked in this match.
    /// As most maps have different height layers. This constructor is only advised if you have a simple plain map with some obstacles on the same height.
    /// </summary>
    public class SimpleMapConstructor
    {
        /// <summary>
        /// Minimal Points that have to be located in a cell to mark it as "walkable" space -> no obstacle
        /// </summary>
        private const int MIN_CELL_QUERY = 1;

        /// <summary>
        /// Map width - width of the grid
        /// </summary>
        public static int MapWidth;
        /// <summary>
        /// Map height - height of the grid
        /// </summary>
        public static int MapHeight;

        /// <summary>
        /// Start Koordinate X from where to begin with grid cell deployment
        /// </summary>
        public static int PosX;

        /// <summary>
        /// Start Koordinate X from where to begin with grid cell deployment
        /// </summary>
        public static int PosY;

        /// <summary>
        /// The grid deployed over the map
        /// </summary>
        public static MapGridCell[][] MapGrid;

        /// <summary>
        /// Lenght of the edges of a square in the mapgrid
        /// </summary>
        public static int CellEdgeLength;

        /// <summary>
        /// Amount of squares -> deploy a grid with cellamount X cellamount cells
        /// </summary>
        public const int Cellamount = 75;

        /// <summary>
        /// This function takes a list of all registered points on the map and tries to
        /// reconstruct a polygonal represenatation of the map with serveral levels
        /// </summary>
        /// <param name="ps"></param>
        public static Map CreateMap(MapMetaData mapmeta, List<Point3D> ps)
        {
            BuildMapGrid(mapmeta);

            var min_z = ps.Min(point => point.Z);
            var max_z = ps.Max(point => point.Z);
            var maplevel = new MapLevel(0, min_z, max_z);
            AssignCellTypes(maplevel, ps.ToArray());

            MapLevel[] maplevels = new MapLevel[] { maplevel }; // This constructor only creates one maplevel

            var map_width_x = ps.Max(point => point.X) - ps.Min(point => point.X);
            var map_width_y = ps.Max(point => point.Y) - ps.Min(point => point.Y);
            Console.WriteLine("Mapwidth in x-Range: " + map_width_x + " Mapwidth in y-Range: " + map_width_y);

            return new Map(map_width_x, map_width_y, maplevels);
        }

        /// <summary>
        /// Create the grid with rectangluar cells
        /// </summary>
        /// <param name="mapmeta"></param>
        private static void BuildMapGrid(MapMetaData mapmeta)
        {
            PosX = PosX = (int)mapmeta.mapcenter_x;
            PosY = PosY = (int)mapmeta.mapcenter_y;

            MapWidth = (int)mapmeta.width;
            MapHeight = (int)mapmeta.height;

            CellEdgeLength = (int)Math.Ceiling((double)MapWidth / Cellamount);

            var currentx = PosX;
            var currenty = PosY;
            var cells = (MapHeight / CellEdgeLength) * (MapWidth / CellEdgeLength);

            MapGrid = new MapGridCell[MapHeight / CellEdgeLength][];

            for (int k = 0; k < MapGrid.Length; k++)
            {
                MapGrid[k] = new MapGridCell[MapWidth / CellEdgeLength];

                for (int l = 0; l < MapGrid[k].Length; l++)
                {
                    MapGrid[k][l] = new MapGridCell
                    {
                        Index_X = k,
                        Index_Y = l,
                        Bounds = new Rectangle2D(new Point2D(currentx, currenty), CellEdgeLength, CellEdgeLength),
                        Blocked = false
                    };
                    currentx += CellEdgeLength;

                }
                currentx = PosX;
                currenty -= CellEdgeLength;
            }
        }



        /// <summary>
        /// Assgins all wall and walkable cells on a maplevel
        /// </summary>
        /// <param name="maplevel"></param>
        private static void AssignCellTypes(MapLevel maplevel, Point3D[] points)
        {
            var count = 0;
            var blcount = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var wrapped = new List<Point3DDataPoint>(); // Wrapp Point3D
            points.ToList().ForEach(p => wrapped.Add(new Point3DDataPoint(p)));

            PointQuadTree<Point3DDataPoint> qtree = new PointQuadTree<Point3DDataPoint>();
            qtree.AddBulk(wrapped);

            for (int k = 0; k < MapGrid.Length; k++)
                for (int l = 0; l < MapGrid[k].Length; l++)
                {
                    var cell = MapGrid[k][l];
                    var rectquery = qtree.GetObjects(cell.Rect); //Get points in a cell
                    if (rectquery.Count >= MIN_CELL_QUERY)
                    {
                        cell.Blocked = true; blcount++;
                        maplevel.WallCells.Add(cell);
                    }
                    else
                    {
                        cell.Blocked = false; count++;
                    }
                }


            Console.WriteLine("Wall cells: " + blcount + " "+ "Free cells: " + count);
            watch.Stop();
            var sec = watch.ElapsedMilliseconds / 1000.0f;
            Console.WriteLine("Time to assign cells: " + sec);
        }
    }
}
