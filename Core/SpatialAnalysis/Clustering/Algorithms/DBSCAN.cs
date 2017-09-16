using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KDTree;
using KDTree.Math;

namespace Clustering
{
    /// <summary>
    /// Main Codeparts from http://codereview.stackexchange.com/questions/108965/implementing-a-fast-dbscan-in-c
    /// </summary>
    public class DBSCAN<T,H> where T : DataPoint<H>, IClusterable<Double, T>, new()
    {
        private T[] datapoints;
        private double epsilon;
        private int minPts;


        public DBSCAN(T[] allPoints, double epsilon, int minPts)
        {
            this.datapoints = allPoints;
            this.epsilon = epsilon;
            this.minPts = minPts;
        }

        public HashSet<T[]> CreateClusters(bool removeoutliers)
        {
            var allPointsDBSCAN = datapoints;

            var kdtree = new KDTree<Double, T>(2, new DoubleMath());
            for (var i = 0; i < allPointsDBSCAN.Length; ++i)
            {
                kdtree.Add(allPointsDBSCAN[i].GetPointDataAsArray(), allPointsDBSCAN[i]);
            }

            var expandcounter = 0;
            for (int i = 0; i < allPointsDBSCAN.Length; i++)
            {
                var p = allPointsDBSCAN[i];
                if (p.isVisited)
                    continue;
                p.isVisited = true;

                T[] neighborPts = RegionQuery(kdtree, p, epsilon);
                if (neighborPts.Length < minPts)
                    p.clusterID = (int)ClusterIDs.Noise;
                else
                {
                    expandcounter++;
                    ExpandCluster(kdtree, p, neighborPts, expandcounter, epsilon, minPts);
                }
            }
            if(removeoutliers)
                return new HashSet<T[]>(
                    allPointsDBSCAN
                        .Where(x => x.clusterID > 0)
                        .GroupBy(x => x.clusterID)
                        .Select(x => x.Select(y => y).ToArray())
                    );
            else
                return new HashSet<T[]>(
                allPointsDBSCAN
                    .GroupBy(x => x.clusterID) //All ClusterIDs
                    .Select(x => x.Select(y => y).ToArray())
                );
        }

        private static void ExpandCluster(KDTree<double, T> tree, T p, T[] neighborPts, int clusterid, double epsilon, int minPts)
        {
            p.clusterID = clusterid;

            var queue = new Queue<T>(neighborPts);
            while (queue.Count > 0)
            {
                var point = queue.Dequeue();
                if (point.clusterID == (int)ClusterIDs.Unclassified)
                {
                    point.clusterID = clusterid;
                }

                if (point.isVisited)
                {
                    continue;
                }

                point.isVisited = true;
                var neighbors = RegionQuery(tree, point, epsilon);
                if (neighbors.Length >= minPts)
                {
                    foreach (var neighbor in neighbors.Where(neighbor => !neighbor.isVisited))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        private static T[] RegionQuery(KDTree<double, T> tree, T p, double epsilon)
        {
            var neighbors = new List<T>();
            var e = tree.RangeQuery(p.GetPointDataAsArray(), epsilon, 10);
            foreach(var entry in e)
                neighbors.Add(entry.Value);
            
            return neighbors.ToArray();
        }
    }

}
