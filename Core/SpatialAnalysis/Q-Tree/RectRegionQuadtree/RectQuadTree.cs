using System.Drawing;
using QuadTree.Common;
using QuadTree.QTreeRectF;
using MathNet.Spatial.Euclidean;

namespace QuadTree
{
    /// <summary>
    /// A QuadTree Object that provides fast and efficient storage of Rectangles in a world space, queried using Rectangles.
    /// </summary>
    /// <typeparam name="T">Any object implementing IQuadStorable.</typeparam>
    public class RectQuadTree<T> : QuadTree<T, RectQuadTreeNode<T, Rectangle2D>, Rectangle2D> where T : IRectQuadTreeStorable
    {
        public RectQuadTree(Rectangle2D rect) : base(rect)
        {
        }

        public RectQuadTree(float x, float y, float width, float height) : base(x, y, width, height)
        {
        }

        public RectQuadTree()
        {
        }

        protected override RectQuadTreeNode<T, Rectangle2D> CreateNode(Rectangle2D rect)
        {
            return new QuadTreeRectFNode<T>(rect);
        }
    }
}
