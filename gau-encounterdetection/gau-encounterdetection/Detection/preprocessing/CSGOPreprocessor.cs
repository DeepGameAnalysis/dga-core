using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameevents;
using Data.Gamestate;
using Data.Gameobjects;
using Data.Utils;
using Clustering;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;
using Detection;

namespace Preprocessing
{
    /// <summary>
    /// The CSGO preprocessor is designed to recreate the map of the gamestate data. Furthermore hashtables are built to perform calculations and predictions
    /// </summary>
    public class CSGOPreprocessor : MainPreprocessor, IPreprocessor
    {
        Hashtable hit_hash;
        Hashtable assist_hash;

        Map map;

        private double ATTACKRANGE_AVERAGE_HURT;
        private double SUPPORTRANGE_AVERAGE_KILL;
        private EventPositionCluster[] attacker_clusters;

        public void PreprocessData(ReplayGamestate gamestate, MapMetaData mapmeta, out EncounterDetectionData edData)
        {
            var nedData = new EncounterDetectionData();
            base.preprocessMain(gamestate,mapmeta, out nedData);

            var ps = new HashSet<Point3D>();
            List<double> hurt_ranges = new List<double>();
            List<double> support_ranges = new List<double>();

            #region Collect positions for preprocessing  and build hashtables of events
            foreach (var round in gamestate.match.rounds)
            {
                foreach (var tick in round.ticks)
                {
                    foreach (var gevent in tick.getTickevents())
                    {
                        switch (gevent.gameeventtype) //Build hashtables with events we need later
                        {
                            case "player_hurt":
                                PlayerHurt ph = (PlayerHurt)gevent;
                                // Remove Z-Coordinate because we later get keys from clusters with points in 2D space -> hashtable needs keys with 2d data
                                hit_hash[ph.actor.Position.ResetZ()] = ph.Victim.Position.ResetZ();
                                hurt_ranges.Add(DistanceFunctions.GetEuclidDistance3D(ph.actor.Position, ph.Victim.Position));
                                continue;
                            case "player_killed":
                                PlayerKilled pk = (PlayerKilled)gevent;
                                hit_hash[pk.actor.Position.ResetZ()] = pk.Victim.Position.ResetZ();
                                hurt_ranges.Add(DistanceFunctions.GetEuclidDistance3D(pk.actor.Position, pk.Victim.Position));

                                if (pk.Assister != null)
                                {
                                    assist_hash[pk.actor.Position.ResetZ()] = pk.Assister.Position.ResetZ();
                                    support_ranges.Add(DistanceFunctions.GetEuclidDistance3D(pk.actor.Position, pk.Assister.Position));
                                }
                                continue;
                        }

                        foreach (var player in gevent.getPlayers())
                        {
                            var vz = player.Velocity.VZ;
                            if (vz == 0) //If player is standing thus not experiencing an acceleration on z-achsis -> TRACK POSITION
                                ps.Add(player.Position);
                            else
                                ps.Add(player.Position.ChangeZ(-54)); // Player jumped -> Z-Value is false -> correct with jumpheight
                        }

                    }
                }
            }
            #endregion
            Console.WriteLine("\nRegistered Positions for Sightgraph: " + ps.Count);

            // Generate 
            this.map = SimpleMapConstructor.createMap(mapmeta, ps);

            if (support_ranges.Count != 0)
                ATTACKRANGE_AVERAGE_HURT = hurt_ranges.Average();
            if (support_ranges.Count != 0)
                SUPPORTRANGE_AVERAGE_KILL = support_ranges.Average();

            // Generate Hurteventclusters
            // Keys of the hashtable are attacker positions, ordering defines a function on how to order the data before performing LEADER
            Func<Point3DDataPoint[], Point3DDataPoint[]> ordering = ops => ops.OrderBy(p => p.clusterPoint.X).ThenBy(p => p.clusterPoint.Y).ToArray();
            var clusterpos = hit_hash.Keys.Cast<Point3D>();
            var wrapped = new List<Point3DDataPoint>(); // Wrapp Point3D to execute Clustering
            clusterpos.ToList().ForEach(p => wrapped.Add(new Point3DDataPoint(p)));

            var leader = new LEADER<Point3DDataPoint>((float)ATTACKRANGE_AVERAGE_HURT, wrapped.ToArray(), ordering);
            var attackerclusters = new List<EventPositionCluster>();

            foreach (var cluster in leader.CreateClusters())
            {
                var extractedpos = new List<Point3D>();
                cluster.data.ForEach(data => extractedpos.Add(data.clusterPoint));
                var attackcluster = new EventPositionCluster(extractedpos.ToArray());
                attackcluster.CalculateClusterRanges(hit_hash);
                attackerclusters.Add(attackcluster);
            }
            this.attacker_clusters = attackerclusters.ToArray();

            edData = nedData;
        }
    }
}
