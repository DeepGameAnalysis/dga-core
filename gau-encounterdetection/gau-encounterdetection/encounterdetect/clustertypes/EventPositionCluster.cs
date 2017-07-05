﻿using System;
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
    /// Cluster a pointcloud of three dimensional event position(for example attacks, heals etc)
    /// </summary>
    public class EventPositionCluster : Cluster<Point3D>
    {
        /// <summary>
        /// Range average of all positions registered in this cluster
        /// </summary>
        public double cluster_range_average { get; set; }

        /// <summary>
        /// Median of all ranges registered in this cluster
        /// </summary>
        public double cluster_range_median { get; set; }

        /// <summary>
        /// Maximal attackrange registered in this cluster
        /// </summary>
        public double max_range { get; set; }

        /// <summary>
        /// Minimal attackrange registered in this cluster
        /// </summary>
        public double min_range { get; set; }

        /// <summary>
        /// Boundings of this cluster as Rectangle
        /// </summary>
        public Rectangle2D boundings { get; set; }

        /// <summary>
        /// The two-dimensional bounding polygon of the datapoints
        /// </summary>
        public Polygon2D boundingpolygon2D { get; set; }

        /// <summary>
        /// The convex hull of all datapoints
        /// </summary>
        public Polygon2D convexhull
        {
            get {
                if(boundingpolygon2D == null) 
                    boundingpolygon2D = new Polygon2D(base.data.Cast<Point2D>()); //Save polygon in case for further use

                return Polygon2D.GetConvexHullFromPoints(base.data.Cast<Point2D>());
            }
        }

        /// <summary>
        /// Constructor delegate to base
        /// </summary>
        /// <param name="data"></param>
        public EventPositionCluster(Point3D[] data) : base(data){ }


        /// <summary>
        /// Calculate all relevant ranges for this cluster
        /// </summary>
        /// <param name="hit_hashtable"></param>
        public void calculateClusterRanges(Hashtable hit_hashtable)
        {
            double[] distances = new double[data.Count];
            int arr_ptr = 0;
            if (data.Count == 0) return;
            foreach (var pos in data)
            {
                Point3D value = (Point3D)hit_hashtable[pos]; // No suitable Z variable no hashtable entry -> null -.-
                distances[arr_ptr] = value.DistanceTo(pos); // Euclidean Distance
                arr_ptr++;
            }

            cluster_range_average = distances.Average();
            cluster_range_median = DistanceFunctions.GetMedian(distances.Cast<float>());
            max_range = distances.Max();
            min_range = distances.Min();
            Console.WriteLine("Attackrange for this cluster is: " + cluster_range_average);
            boundings = getBoundings();
        }

        private Rectangle2D getBoundings()
        {
            throw new NotImplementedException();
        }
    }

}
