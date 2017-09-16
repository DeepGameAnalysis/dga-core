using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clustering;

namespace ClusteringMath
{
    class ClusterMath
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="H">Clusterdatatype for example double</typeparam>
        /// <typeparam name="T">Datatyp we want to cluster</typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T GetClusterCentroid<T>(T[] data) where T : IClusterable<Double, T>
        {
            double clustersumx = 0;
            double clustersumy = 0;
            for (int i = 0; i < data.Length; i++)
            {
                clustersumx += data[i].GetPointDataAsArray()[0];
                clustersumy += data[i].GetPointDataAsArray()[1];
            }
            clustersumx = clustersumx / data.Length;
            clustersumy = clustersumy / data.Length;
            return data[0]; //Change
        }

        /// <summary>
        /// Calculate the Medoid of a array of cluster objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetClusterMedoid<T>(T[] data) where T : IClusterable<Double, T>
        {
            double[] distancesavg = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                double clusterdistanceavg = 0;
                for (int j = 0; j < data.Length; j++)
                    if (i != j)
                        clusterdistanceavg += data[i].GetDistance(data[j]);
                distancesavg[i] = clusterdistanceavg;
            }

            var min = distancesavg[0];
            var min_index = -1;
            for (int i = 1; i < distancesavg.Length; i++)
                if (distancesavg[i] < min)
                {
                    min = distancesavg[i];
                    min_index = i;
                }

            return data[min_index];
        }
    }
}

