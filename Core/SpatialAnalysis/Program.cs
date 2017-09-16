using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using Trajectories;
using KDTree;
using KDTree.Math;

namespace gau_spatial
{
    class Player : IMoveable
    {
        public Vector3D getMovementVector()
        {
            throw new NotImplementedException();
        }

        public Point3D getPosition()
        {
            throw new NotImplementedException();
        }

        public Vector3D getVelocity()
        {
            throw new NotImplementedException();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var pl = new Player();
            var p = new Point3D(22, 2, 9);
            var p2 = new Point3D(2, 2, 9);
            var p3 = new Point3D(22, 2, 19);
            var p4 = new Point3D(22, 12, 9);
            Trajectory<Player> t = new Trajectory<Player>(pl, 2131);
            t.AddPosition(2133, p);
            t.AddPosition(2132, p2);
            t.AddPosition(2134, p3);
            t.AddPosition(2135, p4);
            Console.WriteLine(t);
            Console.WriteLine(t.GetPositionAt(2136));
            Console.ReadLine();

            KDTree<double, Point2D> kd = new KDTree<double, Point2D>(2, new DoubleMath());
            kd.NearestNeighboursQuery(new double[] { 20, 20 }, 2);
        }
    }
}
