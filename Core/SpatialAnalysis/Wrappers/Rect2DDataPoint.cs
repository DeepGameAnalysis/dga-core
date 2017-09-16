using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuadTree.QTreeRectF;
using QuadTree.QTreePoint2D;
using System.Collections;

namespace Clustering
{
    public class Rect2DDataPoint : DataPoint<Rectangle2D>, IRectQuadTreeStorable
    {
        public Rect2DDataPoint(Rectangle2D rect) : base(rect)
        {

        }

        public Rectangle2D Rect => throw new NotImplementedException();
    }
}
