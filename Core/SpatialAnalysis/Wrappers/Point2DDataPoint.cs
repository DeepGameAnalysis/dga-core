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
    public class Point2DDataPoint : DataPoint<Point2D>, IPointQuadTreeStorable, IClusterable<Double, Point2DDataPoint>
    {
        public Point2DDataPoint(Point2D point) : base(point)
        {

        }

        public Point2D ExtractData()
        {
            return clusterPoint;
        }

        public Point2D Point => clusterPoint;

        public Point2DDataPoint AddPointData(double[] v)
        {
            clusterPoint = new Point2D(v);
            return this;
        }

        public double GetDistance(Point2DDataPoint t)
        {
            return DistanceFunctions.GetEuclidDistance2D(t.clusterPoint, clusterPoint);
        }

        public double[] GetPointDataAsArray()
        {
            return new double[] { clusterPoint.X, clusterPoint.Y};
        }
    }
}
