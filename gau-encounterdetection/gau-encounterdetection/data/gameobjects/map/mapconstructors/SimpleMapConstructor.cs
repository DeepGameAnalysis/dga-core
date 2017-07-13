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
    /// </summary>
    public class SimpleMapConstructor
    {
        /// <summary>
        /// Defines the height of a level aka granularity of rastering all height map levels.
        /// </summary>
        private static int rastering_height;

        /// <summary>
        /// Map width - width of the grid
        /// </summary>
        public static int mapdata_width;
        /// <summary>
        /// Map height - height of the grid
        /// </summary>
        public static int mapdata_height;

        /// <summary>
        /// Start Koordinate X from where to begin with grid cell deployment
        /// </summary>
        public static int pos_x;

        /// <summary>
        /// Start Koordinate X from where to begin with grid cell deployment
        /// </summary>
        public static int pos_y;

        /// <summary>
        /// The grid deployed over the map
        /// </summary>
        private static MapGridCell[][] map_grid;

        /// <summary>
        /// Lenght of the edges of a square in the mapgrid
        /// </summary>
        private static int celledge_length;

        /// <summary>
        /// Amount of squares -> deploy a grid with cellamount X cellamount cells
        /// </summary>
        private const int cellamount = 75;

        /// <summary>
        /// This function takes a list of all registered points on the map and tries to
        /// reconstruct a polygonal represenatation of the map with serveral levels
        /// </summary>
        /// <param name="ps"></param>
        public static Map createMap(MapMetaData mapmeta, HashSet<Point3D> ps)
        {
            //TODO Assign right values!!
            pos_x = pos_x = (int)mapmeta.mapcenter_x;
            pos_y = pos_y = (int)mapmeta.mapcenter_y;

            mapdata_width = (int)mapmeta.width;
            mapdata_height = (int)mapmeta.height;

            double length = mapdata_width / cellamount;
            celledge_length = (int)Math.Ceiling(length);

            var currentx = pos_x;
            var currenty = pos_y;
            var cells = (mapdata_height / celledge_length) * (mapdata_width / celledge_length);

            map_grid = new MapGridCell[mapdata_height / celledge_length][];

            for (int k = 0; k < map_grid.Length; k++)
            {
                map_grid[k] = new MapGridCell[mapdata_height / celledge_length];

                for (int l = 0; l < map_grid[k].Length; l++)
                {
                    map_grid[k][l] = new MapGridCell
                    {
                        index_X = k,
                        index_Y = l,
                        bounds = new Rectangle2D(new Point2D(currentx, currenty), celledge_length, celledge_length),
                        blocked = false
                    };
                    currentx += celledge_length;

                }
                currentx = pos_x;
                currenty -= celledge_length;
            }


            // Create the map levels 
            MapLevel[] maplevels = createMapLevels(ps);
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
        private static MapLevel[] createMapLevels(HashSet<Point3D> ps)
        {
            int levelamount = (int)Math.Ceiling((getZRange(ps) / rastering_height));

            MapLevel[] maplevels = new MapLevel[levelamount];

            Console.WriteLine("Levels to create: " + levelamount);
            var min_z = ps.Min(point => point.Z);
            var max_z = ps.Max(point => point.Z);
            Console.WriteLine("From Min Z: " + min_z + " to Max Z: " + max_z);

            for (int i = 0; i < levelamount; i++)
            {
                var upperbound = min_z + (i + 1) * rastering_height;
                var lowerbound = min_z + i * rastering_height;
                var levelps = new HashSet<Point3D>(ps.Where(point => point.Z >= lowerbound && point.Z <= upperbound).OrderBy(point => point.Z));
                Console.WriteLine("Z Range for Level " + i + " between " + lowerbound + " and " + upperbound);

                if (levelps.Count() == 0)
                    throw new Exception("No points on level:" + i);

                Console.WriteLine("Level " + i + ": " + levelps.Count() + " points");
                var ml = new MapLevel(i, lowerbound, upperbound);
                AssignLevelcells(ml, levelps.ToArray());
                maplevels[i] = ml;
            }
            map_grid = null;
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

            ml.level_grid = new MapGridCell[mapdata_height / celledge_length][];
            for (int k = 0; k < ml.level_grid.Length; k++)
                ml.level_grid[k] = new MapGridCell[mapdata_height / celledge_length];


            PointQuadTree<Point3DDataPoint> qtree = new PointQuadTree<Point3DDataPoint>();
            foreach (var cl in ml.clusters)
                    qtree.Add(new Point3DDataPoint(cl));

            for (int k = 0; k < map_grid.Length; k++)
                for (int l = 0; l < map_grid[k].Length; l++)
                {
                    var cell = map_grid[k][l];
                    var rectps = qtree.GetObjects(cell.Rect); //Get points in a cell
                    if (rectps.Count >= MIN_CELL_QUERY)
                    {
                        cell.blocked = false;
                    }
                    else
                    {
                        if (cell.blocked == true) continue; // Prevent already used cells from being assigned to multiple levels
                        cell.blocked = true;
                        map_grid[k][l].blocked = true;
                        ml.walls_tree.Add(cell);
                    }

                    ml.cells_tree.Add(cell.bounds.Center.Data, cell);
                    ml.level_grid[k][l] = cell;
                    count++;
                }
            qtree.Clear();
            ml.cells_tree.Balance();

            Console.WriteLine("Occupied cells by this level: " + count);
            watch.Stop();
            var sec = watch.ElapsedMilliseconds / 1000.0f;
            Console.WriteLine("Time to assign cells: " + sec);
        }

        /// <summary>
        /// Returns Range of Z for this set of points
        /// </summary>
        /// <returns></returns>
        public static double getZRange(HashSet<Point3D> ps)
        {
            return ps.Max(point => point.Z) - ps.Min(point => point.Z);
        }
    }
}
