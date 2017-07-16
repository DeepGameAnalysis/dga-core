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
    /// Rebuild the map by use of positional data. Instead of one level, different levels are built by height of positional data the same way we built one level.
    /// DBSCAN is used to remove outliers.
    /// </summary>
    public class LeveldMapConstructur : SimpleMapConstructor
    {
        /// <summary>
        /// Defines the height of a level aka granularity of rastering all height map levels.
        /// </summary>
        private static int RasteringStep = Player.CSGO_PLAYERMODELL_HEIGHT;

        
        /// <summary>
        /// This function takes a list of all registered points on the map and tries to
        /// reconstruct a polygonal represenatation of the map with serveral levels
        /// </summary>
        /// <param name="ps"></param>
        public static Map CreateMap(MapMetaData mapmeta, List<Point3D> ps)
        {
            PosX = (int)mapmeta.mapcenter_x;
            PosY = (int)mapmeta.mapcenter_y;

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


            // Create the map levels 
            MapLevel[] maplevels = CreateMapLevels(ps);
            var map_width_x = ps.Max(point => point.X) - ps.Min(point => point.X);
            var map_width_y = ps.Max(point => point.Y) - ps.Min(point => point.Y);
            Console.WriteLine("Max x: " + ps.Max(point => point.X) + " Min x: " + ps.Min(point => point.X));
            Console.WriteLine("Mapwidth in x-Range: " + map_width_x + " Mapwidth in y-Range: " + map_width_y);

            return new Map(map_width_x, map_width_y, maplevels);
        }

        /// <summary>
        /// Create a maplevel according to its walkable space.
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        private static MapLevel[] CreateMapLevels(List<Point3D> ps)
        {
            int levelamount = (int)Math.Ceiling((getZRange(ps) / RasteringStep));
            MapLevel[] maplevels = new MapLevel[levelamount];

            Console.WriteLine("Levels to create: " + levelamount);
            var min_z = ps.Min(point => point.Z);
            var max_z = ps.Max(point => point.Z);
            Console.WriteLine("From Min Z: " + min_z + " to Max Z: " + max_z);

            for (int i = 0; i < levelamount; i++)
            {
                var upperbound = min_z + (i + 1) * RasteringStep;
                var lowerbound = min_z + i * RasteringStep;
                var levelps = new HashSet<Point3D>(ps.Where(point => point.Z >= lowerbound && point.Z <= upperbound).OrderBy(point => point.Z));
                Console.WriteLine("Z Range for Level " + i + " between " + lowerbound + " and " + upperbound);

                if (levelps.Count() == 0)
                    throw new Exception("No points on level:" + i);

                Console.WriteLine("Level " + i + ": " + levelps.Count() + " points");
                var ml = new MapLevel(i, lowerbound, upperbound);
                AssignLevelcells(ml, levelps.ToArray());
                maplevels[i] = ml;
            }
            MapGrid = null;
            return maplevels;
        }

        /// <summary>
        /// Minimal Points that have to be located in a cell to mark it as "walkable" space -> no obstacle
        /// </summary>
        private const int MIN_CELL_QUERY = 1;

        /// <summary>
        /// Assgins all wall cells on a maplevel
        /// </summary>
        /// <param name="ml"></param>
        public static void AssignLevelcells(MapLevel ml, Point3D[] points)
        {
            var count = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //var dbscan = new DBSCAN<Point3D>((x, y) => Math.Sqrt(((x.X - y.X) * (x.X - y.X)) + ((x.Y - y.Y) * (x.Y - y.Y))));
            var wrapped = new List<Point3DDataPoint>(); // Wrapp Point3D to execute Clustering
            points.ToList().ForEach(p => wrapped.Add(new Point3DDataPoint(p)));
            var dbscan = new DBSCAN<Point3DDataPoint, Point3D>(wrapped.ToArray(), 60, 2);

            var extractedpos = new List<Point3D>();
            dbscan.CreateClusters(true).ToList().ForEach(data => data.ToList().ForEach(pos => extractedpos.Add(pos.clusterPoint)));

            ml.clusters = extractedpos;
            points = null; // Collect points for garbage

            ml.LevelGrid = new MapGridCell[MapHeight / CellEdgeLength][];
            for (int k = 0; k < ml.LevelGrid.Length; k++)
                ml.LevelGrid[k] = new MapGridCell[MapWidth / CellEdgeLength];


            PointQuadTree<Point3DDataPoint> qtree = new PointQuadTree<Point3DDataPoint>();
            foreach (var cl in ml.clusters)
                qtree.Add(new Point3DDataPoint(cl));

            for (int k = 0; k < MapGrid.Length; k++)
                for (int l = 0; l < MapGrid[k].Length; l++)
                {
                    var cell = MapGrid[k][l];
                    var rectps = qtree.GetObjects(cell.Rect); //Get points in a cell
                    if (rectps.Count >= MIN_CELL_QUERY)
                    {
                        cell.Blocked = false;
                    }
                    else
                    {
                        if (cell.Blocked == true) continue; // Prevent already used cells from being assigned to multiple levels
                        cell.Blocked = true;
                        MapGrid[k][l].Blocked = true;
                        ml.WallCells.Add(cell);
                    }

                    ml.FreeCells.Add(cell.Bounds.Center.GetData(), cell);
                    ml.LevelGrid[k][l] = cell;
                    count++;
                }
            qtree.Clear();
            ml.FreeCells.Balance();

            Console.WriteLine("Occupied cells by this level: " + count);
            watch.Stop();
            var sec = watch.ElapsedMilliseconds / 1000.0f;
            Console.WriteLine("Time to assign cells: " + sec);
        }

        /// <summary>
        /// Returns Range of Z for this set of points
        /// </summary>
        /// <returns></returns>
        public static double getZRange(List<Point3D> ps)
        {
            return ps.Max(point => point.Z) - ps.Min(point => point.Z);
        }
    }
}
