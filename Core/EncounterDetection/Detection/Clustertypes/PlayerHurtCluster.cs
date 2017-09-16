using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;

namespace Clustering
{

    /// <summary>
    /// Cluster player hurt events(and their children events) with their specific data
    /// </summary>
    public class PlayerHurtCluster : Cluster<Point3D>
    {
        /// <summary>
        /// Range average of all positions registered in this cluster
        /// </summary>
        public double AttackRangeAverage { get; set; }

        /// <summary>
        /// Median of all ranges registered in this cluster
        /// </summary>
        public double AttackRangeMedian { get; set; }

        /// <summary>
        /// Maximal attackrange registered in this cluster
        /// </summary>
        public double MaxRange { get; set; }

        /// <summary>
        /// Minimal attackrange registered in this cluster
        /// </summary>
        public double MinRange { get; set; }

        /// <summary>
        /// If true. Cluster was built with attacker positions. If false cluster was built with victimpositions.
        /// </summary>
        public bool IsAttackerCluster { get; set; }

        /// <summary>
        /// Boundings of this cluster as Rectangle
        /// </summary>
        public Rectangle2D Boundings { get; set; }

        /// <summary>
        /// The two-dimensional bounding polygon of the datapoints
        /// </summary>
        public Polygon2D BoundingPolygon2D { get; set; }

        /// <summary>
        /// The convex hull of all datapoints
        /// </summary>
        public Polygon2D ConvexHull
        {
            get
            {
                if (BoundingPolygon2D == null)
                    BoundingPolygon2D = new Polygon2D(base.ClusterData.Cast<Point2D>()); //Save polygon in case for further use

                return Polygon2D.GetConvexHullFromPoints(base.ClusterData.Cast<Point2D>());
            }
        }

        /// <summary>
        /// Constructor delegate to base
        /// </summary>
        /// <param name="data"></param>
        public PlayerHurtCluster(Point3D[] data) : base(data) { }


        /// <summary>
        /// Calculate all relevant ranges for this cluster
        /// </summary>
        /// <param name="hit_hashtable"></param>
        public void CalculateClusterRanges(Hashtable hit_hashtable)
        {
            double[] distances = new double[ClusterData.Count];
            int arr_ptr = 0;
            if (ClusterData.Count == 0) return;
            foreach (var pos in ClusterData)
            {
                Point3D value = (Point3D)hit_hashtable[pos]; // No suitable Z variable no hashtable entry -> null -.-
                distances[arr_ptr] = value.DistanceTo(pos); // Euclidean Distance
                arr_ptr++;
            }

            AttackRangeAverage = distances.Average();
            AttackRangeMedian = StatisticalFunctions.Median(Convert(distances.ToArray()));

            MaxRange = distances.Max();
            MinRange = distances.Min();
            Console.WriteLine("Attackrange for this cluster is: " + AttackRangeAverage);
            Boundings = GetBoundings();
        }

        public static float[] Convert(double[] mtx)
        {
            return mtx.Select(j => (float)j).ToArray();
        }

        private Rectangle2D GetBoundings()
        {
            return new Rectangle2D(ClusterData.Max(point => point.X), ClusterData.Max(point => point.Y), ClusterData.Min(point => point.X), ClusterData.Min(point => point.Y), true);
        }
    }

}
