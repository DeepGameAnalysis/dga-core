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

namespace Preprocessing
{
    /// <summary>
    /// The CSGO preprocessor is designed to recreate the map of the gamestate data. Furthermore hashtables are built to perform calculations and predictions
    /// </summary>
    public class CSGOPreprocessor : IPreprocessor
    {
        Hashtable hit_hashtable;
        Hashtable assist_hashtable;

        Map map;

        private double ATTACKRANGE_AVERAGE_HURT;
        private double SUPPORTRANGE_AVERAGE_KILL;
        private AttackerCluster[] attacker_clusters;

        public void preprocessData(ReplayGamestate gamestate, MapMetaData mapmeta)
        {

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
                                hit_hashtable[ph.actor.position.ResetZ()] = ph.victim.position.ResetZ();
                                hurt_ranges.Add(DistanceFunctions.GetEuclidDistance3D(ph.actor.position, ph.victim.position));
                                continue;
                            case "player_killed":
                                PlayerKilled pk = (PlayerKilled)gevent;
                                hit_hashtable[pk.actor.position.ResetZ()] = pk.victim.position.ResetZ();
                                hurt_ranges.Add(DistanceFunctions.GetEuclidDistance3D(pk.actor.position, pk.victim.position));

                                if (pk.assister != null)
                                {
                                    assist_hashtable[pk.actor.position.ResetZ()] = pk.assister.position.ResetZ();
                                    support_ranges.Add(DistanceFunctions.GetEuclidDistance3D(pk.actor.position, pk.assister.position));
                                }
                                continue;
                        }

                        foreach (var player in gevent.getPlayers())
                        {
                            var vz = player.velocity.VZ;
                            if (vz == 0) //If player is standing thus not experiencing an acceleration on z-achsis -> TRACK POSITION
                                ps.Add(player.position);
                            else
                                ps.Add(player.position.ChangeZ(-54)); // Player jumped -> Z-Value is false -> correct with jumpheight
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
            Func<Point3D[], Point3D[]> ordering = ops => ops.OrderBy(p => p.X).ThenBy(p => p.Y).ToArray();

            var leader = new LEADER<Point3D>((float)ATTACKRANGE_AVERAGE_HURT, hit_hashtable.Keys.Cast<Point3D>().ToArray(), ordering);
            var attackerclusters = new List<AttackerCluster>();

            foreach (var cluster in leader.createClusters())
            {
                var attackcluster = new AttackerCluster(cluster.data.ToArray());
                attackcluster.calculateClusterAttackranges(hit_hashtable);
                attackerclusters.Add(attackcluster);
            }
            this.attacker_clusters = attackerclusters.ToArray();
        }
    }
}
