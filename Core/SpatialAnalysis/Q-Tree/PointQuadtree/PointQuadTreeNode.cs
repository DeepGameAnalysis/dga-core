using System.Diagnostics;
using System.Drawing;
using QuadTree.Common;
using QuadTree.QTreeRectF;
using MathNet.Spatial.Euclidean;

namespace QuadTree.QTreePoint2D
{
    /// <summary>
    /// A QuadTree Object that provides fast and efficient storage of objects in a world space.
    /// </summary>
    /// <typeparam name="T">Any object implementing IQuadStorable.</typeparam>
    public class PointQuadTreeNode<T> : QuadTreeFNodeCommon<T, PointQuadTreeNode<T>> where T : IPointQuadTreeStorable
    {
        public PointQuadTreeNode(Rectangle2D rect)
            : base(rect)
        {
        }

        public PointQuadTreeNode(float x, float y, float width, float height)
            : base(x, y, width, height)
        {
        }

        internal PointQuadTreeNode(PointQuadTreeNode<T> parent, Rectangle2D rect)
            : base(parent, rect)
        {
        }

        protected override PointQuadTreeNode<T> CreateNode(Rectangle2D Rectangle2D)
        {
            VerifyNodeAssertions(Rectangle2D);
            return new PointQuadTreeNode<T>(this, Rectangle2D);
        }

        protected override bool CheckContains(Rectangle2D rect, T data)
        {
            return rect.Contains(data.Point);
        }

        public override bool ContainsObject(QuadTreeObject<T, PointQuadTreeNode<T>> qto)
        {
            return CheckContains(QuadRect, qto.Data);
        }

        protected override bool CheckIntersects(Rectangle2D searchRect, T data)
        {
            return CheckContains(searchRect, data);
        }

        protected override Point2D GetMortonPoint(T p)
        {
            return p.Point;
        }
    }
}