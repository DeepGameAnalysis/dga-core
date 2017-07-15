using System;
using System.Collections.Generic;
using System.Linq;
using QuadTree;
using KDTree;
using KDTree.Math;
using MathNet.Spatial.Euclidean;

namespace Data.Gameobjects
{
    public class Map
    {
        public static string[] SUPPORTED_MAPS = new string[] { "de_dust2" , "de_cbble","de_cache","de_mirage","de_inferno", "de_overpass" };

        /// <summary>
        /// Array holding the different maplevels ordered from lowest level (f.e. tunnels beneath the ground) to highest (2nd floor etc)
        /// </summary>
        public MapLevel[] maplevels;

        /// <summary>
        /// All obstacles of a map which are dynamic in their appearance and/or position
        /// </summary>
        public HashSet<MapObstacle> dynamic_obstacles;

        /// <summary>
        /// Dictionary holding the maplevel of player (where he is located)
        /// </summary>
        public Dictionary<long, MapLevel> Levels = new Dictionary<long, MapLevel>();

        /// <summary>
        /// Width in x range
        /// </summary>
        private double width;

        /// <summary>
        /// Width in y range
        /// </summary>
        private double height;



        public Map(double width, double height, MapLevel[] maplevels)
        {
            this.width = width;
            this.height = height;
            this.maplevels = maplevels;
            this.dynamic_obstacles = new HashSet<MapObstacle>();
        }

        /// <summary>
        /// Update the mapdata cause a dynamic object on the map changed
        /// </summary>
        public void updateMap()
        {

        }

        /// <summary>
        /// Returns if this player is standing on the level
        /// </summary>
        /// <returns></returns>
        public MapLevel FindLevelFromPlayer(Player p)
        {
            var vz = p.Velocity.VZ;
            double pz = p.Position.Z;
            if (vz != 0)
                pz -= Player.CSGO_PLAYERMODELL_JUMPHEIGHT; // Substract jumpheight to get real z coordinate(see process data)
            foreach (var level in maplevels)
            {
                if (pz <= level.max_z && pz >= level.min_z || (pz == level.max_z || pz == level.min_z))
                    return level;
            }

            foreach (var level in maplevels)
            {
                Console.WriteLine(level.max_z + " " + level.min_z);
            }
            throw new Exception("Could not find Level for: " + p + " " + pz);
            // This problem occurs because: Positions where player had z-velocity had been sorted out
            // Then we built the levels. If now such a player wants to know his level but current levels dont capture
            // This position because it was sorted out -> no suitable level found
        }


        /// <summary>
        /// Returns a bounding box of the map with root at 0,0
        /// </summary>
        /// <returns></returns>
        public Rectangle2D getMapBoundingBox()
        {
            return new Rectangle2D(new Point2D(0,0), width, height);
        }


        /// <summary>
        /// Returns arry of indices maplevel of all maplevels this players aimvector has clipped
        /// </summary>
        /// <param name="player"></param>
        /// <param name="player_maplevel"></param>
        /// <returns></returns>
        internal MapLevel[] getClippedLevels(int starth, int endh)
        {
            //Console.WriteLine("Start level: "+starth + " Endlevel: "+endh);
            var level_diff = Math.Abs(starth - endh);
            if (level_diff == 1) return new MapLevel[] { maplevels[endh] };

            MapLevel[] clipped_levels = new MapLevel[level_diff];

            int index = 0;
            if (starth < endh) // Case: p1 is standing on a level below p2
                for (int i = starth + 1; i <= endh; i++)
                {
                    clipped_levels[index] = maplevels[i];
                    index++;
                }
            else if (starth > endh) // Case: p2 is standing on a level below p1
                for (int i = starth - 1; i >= endh; i--)
                {
                    clipped_levels[index] = maplevels[i];
                    index++;
                }
            if (clipped_levels.Count() == 0) throw new Exception("Clipped Maplevel List cannot be empty");

            foreach (var m in clipped_levels)
            {
                if (m == null) throw new Exception("Clipped Maplevel cannot be null");
                //Console.WriteLine("Clipped Level: " + m.height);
            }
            return clipped_levels;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ml_height"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public MapLevel nextLevel(int ml_height, int dir)
        {
            if (dir != 1 || dir != -1) throw new Exception("Direction not matching");
            try
            {
                return maplevels[ml_height + dir];
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not find next level");
                Console.WriteLine(e.Message);
                return null;
            }
        }

    }

    /// <summary>
    /// Class representing all points of same height in a map thus describing the obstacles at this height
    /// </summary>
    public class MapLevel
    {


        /// <summary>
        /// Clusters containing the points describing this level
        /// </summary>
        public List<Point3D> clusters;

        /// <summary>
        /// Array holding all grid cells 
        /// </summary>
        public MapGridCell[][] LevelGrid;

        /// <summary>
        /// All map cells representing obstacles and walls on this level - maybe use kdtree for nearest neighbors
        /// </summary>
        public KDTree<double, MapGridCell> FreeCells = new KDTree<double, MapGridCell>(2, new DoubleMath());

        /// <summary>
        /// All cells representing walls in a quadtree
        /// </summary>
        public RectQuadTree<MapGridCell> WallCells = new RectQuadTree<MapGridCell>();

        /// <summary>
        /// Height of this level on the map - > 0 lowest level 
        /// </summary>
        public int HeightIndex;

        /// <summary>
        /// Min and Max z-Koordinate occuring in walkable positions registered
        /// </summary>
        public double min_z, max_z;

        public MapLevel(int heightindex, double min_z, double max_z)
        {
            this.max_z = max_z;
            this.min_z = min_z;
            this.HeightIndex = heightindex;
        }

        public override string ToString()
        {
            return "Level: " + HeightIndex + " From: " + min_z + " To " + max_z;
        }
    }

    public class MapObstacle
    {

    }


}
