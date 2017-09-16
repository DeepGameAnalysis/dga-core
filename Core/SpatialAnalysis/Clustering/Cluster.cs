using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using MathNet.Spatial.Euclidean;

/// <summary>
/// TODO: make generic some time
/// </summary>
namespace Clustering 
{
    public class Cluster<T>
    {
        /// <summary>
        /// Data points assigned to this cluster
        /// </summary>
        public List<T> ClusterData;

        /// <summary>
        /// Centroid or Medoid of this cluster - can either be part of data or not
        /// </summary>
        public T representative;

        /// <summary>
        /// Create an empty cluster - add datapoints
        /// </summary>
        public Cluster()
        {
            this.ClusterData = new List<T>();
        }

        /// <summary>
        /// Create a cluster with three-dimensional data
        /// </summary>
        /// <param name="data"></param>
        public Cluster(T[] data)
        {
            this.ClusterData = data.ToList();
        }

        /// <summary>
        /// Create cluster with one starting datapoint
        /// </summary>
        /// <param name="datapoint"></param>
        public Cluster(T datapoint)
        {
            this.ClusterData = new List<T>();
            assignToCluster(datapoint);
        }

        /// <summary>
        /// Add a datapoint to the cluster
        /// </summary>
        /// <param name="p"></param>
        internal void assignToCluster(T p)
        {
            ClusterData.Add(p);
        }


        //It's the point with the smallest average distance to all other points.

    }
}
