using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using QuadTree.QTreeRectF;
namespace Data.Gameobjects
{
    public class MapGrid
    {
    }

    public class MapGridCell : IRectQuadTreeStorable
    {
        /// <summary>
        /// Index x of the mapcell in the map grid
        /// </summary>
        public int Index_X { get; set; }

        /// <summary>
        /// Index y of the mapcell in the map grid
        /// </summary>
        public int Index_Y { get; set; }

        /// <summary>
        /// Is this cell already occupied -> it has not been walked by a player
        /// </summary>
        public bool Blocked { get; set; }

        /// <summary>
        /// Rectangular respresentation of the cell
        /// </summary>
        public Rectangle2D Bounds { get; set; }

        public Rectangle2D Rect
        {
            get
            {
                return Bounds;
            }
        }

    }

    /// <summary>
    /// Class for fast queries on maprgridcells to determine free sight or other relations TODO: maybe split into different queries
    /// </summary>
    public class MapGridCellQuery
    {
        /// <summary>
        /// Queried cell 
        /// </summary>
        public MapGridCell Cell { get; set; }

        /// <summary>
        /// Possible yaw for a FOV Query
        /// </summary>
        public float Yaw { get; set; }
    }
}