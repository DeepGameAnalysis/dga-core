using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clustering
{
    /// <summary>
    /// Identify a cluster
    /// </summary>
    public enum ClusterIDs
    {
        Unclassified = 0,
        Noise = -1
    }

    public class DataPoint<T>
    {
        /// <summary>
        /// Has this datapoint be visited by an algorithm accessing this variable
        /// </summary>
        public bool isVisited;

        /// <summary>
        /// Data type wrapped
        /// </summary>
        public T clusterPoint;

        /// <summary>
        /// 
        /// </summary>
        public int clusterID;

        public DataPoint(T point)
        {
            clusterPoint = point;
            isVisited = false;
            clusterID = (int)ClusterIDs.Unclassified;
        }

        public DataPoint() { }

    }
}
