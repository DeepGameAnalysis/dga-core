using System.Drawing;
using MathNet.Spatial.Euclidean;

namespace QuadTree.QTreePoint2D
{
    /// <summary>
    /// Interface to define Rect, so that QuadTree knows how to store the object.
    /// </summary>
    public interface IPointQuadTreeStorable
    {
        /// <summary>
        /// The Point2D that defines the object's boundaries.
        /// </summary>
        Point2D Point { get; }
    }
}