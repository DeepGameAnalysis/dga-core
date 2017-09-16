using System;
using System.Collections.Generic;
using System.Linq;

namespace Clustering
{
    public class LEADER<T>  where T : IClusterable<Double, T>
    {
        /// <summary>
        /// Delta. Determines when an object belongs to a cluster or creates a new one if distance between data and leader is greater delta
        /// </summary>
        public float Delta { get; set; }

        /// <summary>
        /// Cluster the given data with LEADER. Pass a function to order the data as u wish
        /// </summary>
        /// <param name="datapoints"></param>
        public LEADER(float delta, T[] datapoints, Func<T[], T[]> order)
        {
            this.datapoints = datapoints;
            this.Delta = delta;
            this.order = order;
        }

        //
        // Helpers. Throw away if possible
        //
        private List<Cluster<T>> clusters = new List<Cluster<T>>();

        private List<T> leaders = new List<T>();

        private Func<T[], T[]> order;

        private T[] datapoints;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Cluster<T>[] CreateClusters()
        {
            clusters.Clear();
            datapoints = order(datapoints); //Use the passed order function to ensure the wished execution of LEADER
            Cluster<T> start_cluster = new Cluster<T>(datapoints[0]);
            int leader_ind = 0;
            leaders.Add(datapoints.First());

            clusters.Add(start_cluster);

            for (int i = 1; i < datapoints.Length; i++)
            {
                var datapoint = datapoints[i];

                var min_dist = 0.0;
                var min_dist_leader_index = 0;
                for (int index = 0; index < leaders.Count; index++)
                {
                    var leader_distance = leaders[index].GetDistance(datapoint);
                    if (index == 0) {
                        min_dist = leader_distance;
                        continue;
                    }
                    if (leader_distance < min_dist)
                    {
                        min_dist = leader_distance;
                        min_dist_leader_index = index;
                    }
                }

                if (min_dist < Delta)
                {
                    clusters[min_dist_leader_index].assignToCluster(datapoint);
                }
                else
                {
                    Cluster<T> new_cluster = new Cluster<T>(datapoint);
                    clusters.Add(new_cluster);
                    leader_ind++;
                    leaders.Add(datapoint);
                }
            }
            leaders.Clear();
            var arr = clusters.ToArray();
            clusters.Clear();
            return arr;
        }

    }
}
