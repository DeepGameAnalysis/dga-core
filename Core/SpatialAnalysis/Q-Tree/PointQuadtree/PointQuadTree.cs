using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using QuadTree.Common;
using QuadTree.QTreePoint2D;
using MathNet.Spatial.Euclidean;

/// <summary>
/// https://github.com/splitice/QuadTrees - MIT License
/// </summary>
namespace QuadTree
{
    /// <summary>
    /// A QuadTree Object that provides fast and efficient storage of Points in a world space, queried using Rectangles.
    /// </summary>
    /// <typeparam name="T">Any object implementing IQuadStorable.</typeparam>
    public class PointQuadTree<T> : QuadTree<T, PointQuadTreeNode<T>, Rectangle2D> where T : IPointQuadTreeStorable
    {
        public PointQuadTree(Rectangle2D rect)
            : base(rect)
        {
        }

        public PointQuadTree(float x, float y, float width, float height)
            : base(x, y, width, height)
        {
        }

        public PointQuadTree()
            : base()
        {

        }

        protected override PointQuadTreeNode<T> CreateNode(Rectangle2D rect)
        {
            return new PointQuadTreeNode<T>(rect);
        }
    }
}
