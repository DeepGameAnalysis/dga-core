using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;
using Data.Gameobjects;

namespace CollisionManager
{
    public class CollisionController
    {
        /// <summary>
        /// Test if a vetor from actor to reciever collides with a rect representing a wall or obstacle.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="level_cells"></param>
        /// <returns></returns>
        public static Point3D LOSIntersectsObstacle2D(Point3D start, Point3D end, MapLevel maplevel)
        {
            //return BresenhamLineStepping(start, end, maplevel);
            if (maplevel == null) throw new Exception("Maplevel cannot be null");

            //Quadtree reduces cells to test depending on the rectangle formed by actorps and recieverpos -> players are close -> far less cells
            var searchrect = GridFunctions.GetRectFromPoints(start, end);

            var queriedRects = maplevel.walls_tree.GetObjects(searchrect.getAsQuadTreeRect());

            foreach (var wallcell in queriedRects.OrderBy(r => Math.Abs(r.Center.X - start.X)).ThenBy(r => Math.Abs(r.Center.Y - start.Y))) //Order Rectangles by distance to the actor. 
            {
                var intersection_point = wallcell.Rect2DIntersectsLine(start, end);
                if (intersection_point != null)
                    return intersection_point;
            }
            return null;
        }
    }
}
