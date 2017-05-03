using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameevents;
using Data.Gamestate;
using Data.Utils;
using GAUMath.Library;
using Clustering;

namespace Preprocessing
{
    /// <summary>
    /// The CSGO preprocessor is designed to recreate the map of the gamestate data. Furthermore hashtables are built to perform calculations and predictions
    /// </summary>
    public class CSGOPreprocessor : IPreprocessor
    {

        public void preprocessData(Gamestate gamestate)
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
                                hurt_ranges.Add(ExtendedMath.GetEuclidDistance2D(ph.actor.position, ph.victim.position));
                                continue;
                            case "player_killed":
                                PlayerKilled pk = (PlayerKilled)gevent;
                                hit_hashtable[pk.actor.position.ResetZ()] = pk.victim.position.ResetZ();
                                hurt_ranges.Add(ExtendedMath.GetEuclidDistance2D(pk.actor.position, pk.victim.position));

                                if (pk.assister != null)
                                {
                                    assist_hashtable[pk.actor.position.ResetZ()] = pk.assister.position.ResetZ();
                                    support_ranges.Add(ExtendedMath.GetEuclidDistance2D(pk.actor.position, pk.assister.position));
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
            this.map = SimpleMapBuilder.createMap(mapmeta, ps);

            if (support_ranges.Count != 0)
                ATTACKRANGE_AVERAGE_HURT = hurt_ranges.Average();
            if (support_ranges.Count != 0)
                SUPPORTRANGE_AVERAGE_KILL = support_ranges.Average();

            // Generate Hurteventclusters
            var leader = new LEADER<float>((float)ATTACKRANGE_AVERAGE_HURT);
            var attackerclusters = new List<AttackerCluster>();
            foreach (var cluster in leader.createClusters(hit_hashtable.Keys.Cast<EDVector3D>().ToList()))
            {
                var attackcluster = new AttackerCluster(cluster.data.ToArray());
                attackcluster.calculateClusterAttackrange(hit_hashtable);
                attackerclusters.Add(attackcluster);
            }
            this.attacker_clusters = attackerclusters.ToArray();
        }
    }
}
