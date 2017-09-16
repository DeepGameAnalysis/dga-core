using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clustering
{
    /// <summary>
    /// Interface for clustering data of a certain object.
    /// H is the datatype the clustering is working on and
    /// which T has to return. So if T is a Point3D we might need a double[] from it to cluster -> all coordinates of the point
    /// </summary>
    /// <typeparam name="H"></typeparam>
    public interface IClusterable<H,T> //todo: maybe abstract class cause constructor is needed? see KMEANs
    {
        /// <summary>
        /// Get the data of the clusterable object as a H array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        H[] GetPointDataAsArray();

        /// <summary>
        /// Most algorithms return a array of values from type H as outcome for a cluster object of type T.
        /// AddData determines how the array is parsed into the Object of another type T
        /// </summary>
        /// <param name="v"></param>
        T AddPointData(H[] v);

        /// <summary>
        /// A distance function for the clusterable object. Measure distance between to object of the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        H GetDistance(T t); //todo: maybe use Func

        /// <summary>
        /// Most algorithms return a array of values from type H as outcome for a cluster object of type T.
        /// AddData determines how the array is parsed into the Object of another type T
        /// </summary>
        /// <param name="v"></param>
        //T CreatePoint(H[] v);

        /// <summary>
        /// For data types such as string there is the need to weight the string and return an H to determine its "value"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        //H Weight(T t);
    }
}
