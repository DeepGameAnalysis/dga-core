using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clustering
{
    public class OPTICS<T> where T : IClusterable<Double, T>
    {
        private T[] datapoints;
        private double epsilon;
        private int minPts;


        public OPTICS(T[] allPoints, double epsilon, int minPts)
        {
            this.datapoints = allPoints;
            this.epsilon = epsilon;
            this.minPts = minPts;
        }

        public HashSet<T[]> createClusters(bool removeoutliers)
        {
            //foreach (var p in datapoints)
                //p.reachabilitydistance = UNDEFINED;

            //foreach (var p in datapoints)

                /*N = getNeighbors(p, eps)
                mark p as processed
       output p to the ordered list
       Seeds = empty priority queue
       if (core - distance(p, eps, Minpts) != UNDEFINED)
                update(N, p, Seeds, eps, Minpts)
          for each next q in Seeds
             N' = getNeighbors(q, eps)
             mark q as processed
             output q to the ordered list
             if (core - distance(q, eps, Minpts) != UNDEFINED)
                    update(N', q, Seeds, eps, Minpts)*/
            return null;
        }

        private void update(T[] n, T p, Queue<T> seeds)
        {
/*    coredist = core - distance(p, eps, MinPts)
    for each o in N
       if (o is not processed)
          new- reach - dist = max(coredist, dist(p, o))
          if (o.reachability - distance == UNDEFINED) // o is not in Seeds
                o.reachability - distance = new- reach - dist
              Seeds.insert(o, new- reach - dist)
          else               // o in Seeds, check for improvement
              if (new- reach - dist < o.reachability - distance)
                o.reachability - distance = new- reach - dist
                 Seeds.move - up(o, new- reach - dist)*/
        }

    }
}
