using System.Drawing;
using QuadTree.Common;
using MathNet.Spatial.Euclidean;

namespace QuadTree.QTreeRectF
{
    /// <summary>
    /// A QuadTree Object that provides fast and efficient storage of objects in a world space.
    /// </summary>
    /// <typeparam name="T">Any object implementing IQuadStorable.</typeparam>
    public abstract class RectQuadTreeNode<T, TQuery> : QuadTreeFNodeCommon<T, RectQuadTreeNode<T, TQuery>, TQuery> where T : IRectQuadTreeStorable
    {
        public RectQuadTreeNode(Rectangle2D rect) : base(rect)
        {
        }

        public RectQuadTreeNode(int x, int y, int width, int height) : base(x, y, width, height)
        {
        }

        internal RectQuadTreeNode(RectQuadTreeNode<T, TQuery> parent, Rectangle2D rect) : base(parent, rect)
        {
        }

        protected override bool CheckContains(Rectangle2D rect, T data)
        {
            return rect.Contains(data.Rect);
        }
    }

    public class QuadTreeRectFNode<T> : RectQuadTreeNode<T, Rectangle2D> where T : IRectQuadTreeStorable
    {
        public QuadTreeRectFNode(Rectangle2D rect) : base(rect)
        {
        }

        public QuadTreeRectFNode(int x, int y, int width, int height) : base(x, y, width, height)
        {
        }

        internal QuadTreeRectFNode(QuadTreeRectFNode<T> parent, Rectangle2D rect) : base(parent, rect)
        {
        }
        protected override RectQuadTreeNode<T, Rectangle2D> CreateNode(Rectangle2D Rectangle2D)
        {
            VerifyNodeAssertions(Rectangle2D);
            return new QuadTreeRectFNode<T>(this, Rectangle2D);
        }

        protected override bool CheckIntersects(Rectangle2D searchRect, T data)
        {
            return searchRect.Intersects(data.Rect);
        }

        public override bool ContainsObject(QuadTreeObject<T, RectQuadTreeNode<T, Rectangle2D>> qto)
        {
            return CheckContains(QuadRect, qto.Data);
        }

        protected override bool QueryContains(Rectangle2D search, Rectangle2D rect)
        {
            return search.Contains(rect);
        }

        protected override bool QueryIntersects(Rectangle2D search, Rectangle2D rect)
        {
            return search.Intersects(rect);
        }
        protected override Point2D GetMortonPoint(T p)
        {
            return p.Rect.UpperLeftPoint;//todo: center?
        }
    }
}