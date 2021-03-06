﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;
using Data.Gameobjects;

namespace CollisionManager
{
    /// <summary>
    /// Manages all collisions which need testing and are not performed within geometry classes in the math library
    /// </summary>
    public class CollisionController
    {
        /// <summary>
        /// Test if a possible line of sight (PLOS) from actor to reciever collides with a rect representing a wall or obstacle.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="level_cells"></param>
        /// <returns></returns>
        public static Point2D? PLOSIntersectsMap2D(Point2D start, Point2D end, MapLevel maplevel)
        {
            //TODO: Not working because searchrect is empty
            //return BresenhamLineStepping(start, end, maplevel);
            if (maplevel == null) throw new Exception("Maplevel cannot be null");

            //Quadtree reduces cells to test depending on the rectangle formed by actorps and recieverpos -> players are close -> far less cells

            var searchrect = GridFunctions.GetRectFromPoints(start, end);

            var queriedRects = maplevel.WallCells.GetObjects(searchrect);

            foreach (var wallcell in queriedRects.OrderBy(cell => Math.Abs(cell.Bounds.X - start.X)).ThenBy(cell => Math.Abs(cell.Bounds.Y - start.Y))) //Order Rectangles by distance to the actor. 
            {
                var intersection_point = wallcell.Bounds.Rect2DIntersectsLine(start, end);
                if (intersection_point != null)
                    return intersection_point;
            }
            return null;
        }
    }
}
