using System.Drawing;
using MathNet.Spatial.Euclidean;

namespace QuadTree.QTreeRectF
{
    /// <summary>
    /// Interface to define Rect, so that QuadTree knows how to store the object.
    /// </summary>
    public interface IRectQuadTreeStorable
    {
        /// <summary>
        /// The Rectangle2D that defines the object's boundaries.
        /// </summary>
        Rectangle2D Rect { get; }
    }
}