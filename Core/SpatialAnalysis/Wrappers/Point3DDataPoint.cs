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
    public class Point3DDataPoint : DataPoint<Point3D>, IPointQuadTreeStorable, IClusterable<Double, Point3DDataPoint>
    {

        public Point3DDataPoint(Point3D point) : base(point)
        {
        }

        public Point3DDataPoint(){}

        public Point3D ExtractData()
        {
            return clusterPoint;
        }

        public Point2D Point => clusterPoint.SubstractZ(); //todo: Makes no sense. True 3D points should be stored in octree

        public Point3DDataPoint AddPointData(double[] v)
        {
            clusterPoint = new Point3D(v);
            return this;
        }

        public double GetDistance(Point3DDataPoint t)
        {
            return DistanceFunctions.GetEuclidDistance3D(t.clusterPoint, clusterPoint);
        }

        public double[] GetPointDataAsArray()
        {
            return clusterPoint.GetData();
        }
    }
}