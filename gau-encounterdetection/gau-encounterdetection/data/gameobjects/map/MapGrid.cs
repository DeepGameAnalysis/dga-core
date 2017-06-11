using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Gameobjects
{
    public class MapGrid
    {
    }

    public class MapGridCell : EDRect
    {
        /// <summary>
        /// Index x of the mapcell in the map grid
        /// </summary>
        public int index_X { get; set; }

        /// <summary>
        /// Index y of the mapcell in the map grid
        /// </summary>
        public int index_Y { get; set; }

        /// <summary>
        /// Is this rect already occupied as grid cell -> it has not been walked by a player
        /// </summary>
        public bool blocked { get; set; }

        public MapGridCell Copy()
        {
            return new MapGridCell
            {
                index_X = index_X,
                index_Y = index_Y,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                blocked = false
            };
        }

        public override string ToString()
        {
            return "x: " + X + " y: " + Y + " width: " + Width + " height: " + Height + " index x: " + index_X + " index y: " + index_Y;
        }
    }

    public class MapGridCellQuery
    {
        public MapGridCell cell { get; set; }
        public float yaw { get; set; }
    }
}