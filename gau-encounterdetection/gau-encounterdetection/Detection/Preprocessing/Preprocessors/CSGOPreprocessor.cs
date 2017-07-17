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

        public void PreprocessData(ReplayGamestate gamestate, MapMetaData mapmeta, EncounterDetectionData edData)
        {
            // Load and create standard data
            base.PreprocessMain(gamestate,mapmeta, edData);

            // Custom preprocessing for your game

            #region Collect positions for preprocessing and build hashtables of events

            var ps = new HashSet<Point3D>();
            List<double> hurt_ranges = new List<double>();
            List<double> support_ranges = new List<double>();

            foreach (var round in gamestate.match.rounds)
            {
                foreach (var tick in round.ticks)
                {
                    foreach (var gevent in tick.getTickevents())
                    {

                        switch (gevent.GameeventType) //Build hashtables with events we need later
                        {
                            case "player_hurt":
                                PlayerHurt ph = (PlayerHurt)gevent;
                                // Remove Z-Coordinate because we later get keys from clusters with points in 2D space -> hashtable needs keys with 2d data
                                edData.Hit_hashtable[ph.Actor.Position.ResetZ()] = ph.Victim.Position.ResetZ();
                                hurt_ranges.Add(DistanceFunctions.GetEuclidDistance3D(ph.Actor.Position, ph.Victim.Position));
                                continue;
                            case "player_killed":
                                PlayerKilled pk = (PlayerKilled)gevent;
                                edData.Hit_hashtable[pk.Actor.Position.ResetZ()] = pk.Victim.Position.ResetZ();
                                hurt_ranges.Add(DistanceFunctions.GetEuclidDistance3D(pk.Actor.Position, pk.Victim.Position));

                                if (pk.Assister != null)
                                {
                                    edData.DirectAssist_hashtable[pk.Actor.Position.ResetZ()] = pk.Assister.Position.ResetZ();
                                    support_ranges.Add(DistanceFunctions.GetEuclidDistance3D(pk.Actor.Position, pk.Assister.Position));
                                }
                                continue;
                        }

                        foreach (var player in gevent.getPlayers())
                        {
                            var vz = player.Velocity.VZ;
                            if (vz == 0) //If player is standing thus not experiencing an acceleration on z-achsis -> TRACK POSITION
                                ps.Add(player.Position);
                            else
                                ps.Add(player.Position.ChangeZ(-Player.CSGO_PLAYERMODELL_JUMPHEIGHT)); // Player jumped -> Z-Value is false -> correct with jumpheight
                        }

                    }
                }
            }
            #endregion

            Console.WriteLine("\nRegistered Positions for Sightgraph: " + ps.Count);
            Console.WriteLine("\nRegistered Hits: " + edData.Hit_hashtable.Count);

            // Generate Map with a constructor
            edData.Map = SimpleMapConstructor.CreateMap(mapmeta, ps.ToList());

            if (support_ranges.Count != 0)
                edData.ATTACKRANGE_AVERAGE = hurt_ranges.Average();
            if (support_ranges.Count != 0)
                edData.SUPPORTRANGE_AVERAGE = support_ranges.Average();

            // Generate Hurteventclusters
            // Keys of the hashtable are attacker positions, ordering defines a function on how to order the data before performing LEADER
            Func<Point3DDataPoint[], Point3DDataPoint[]> ordering = ops => ops.OrderBy(p => p.clusterPoint.X).ThenBy(p => p.clusterPoint.Y).ToArray();
            var clusterpos = edData.Hit_hashtable.Keys.Cast<Point3D>();
            var wrapped = new List<Point3DDataPoint>(); // Wrapp Point3D to execute Clustering
            clusterpos.ToList().ForEach(p => wrapped.Add(new Point3DDataPoint(p)));

            var leader = new LEADER<Point3DDataPoint>((float)edData.ATTACKRANGE_AVERAGE, wrapped.ToArray(), ordering);
            var attackerclusters = new List<EventPositionCluster>();

            foreach (var cluster in leader.CreateClusters())
            {
                var extractedpos = new List<Point3D>();
                cluster.ClusterData.ForEach(data => extractedpos.Add(data.clusterPoint));
                var attackcluster = new EventPositionCluster(extractedpos.ToArray());
                attackcluster.CalculateClusterRanges(edData.Hit_hashtable);
                attackerclusters.Add(attackcluster);
            }
            edData.PlayerHurt_clusters = attackerclusters.ToArray();
        }
    }
}
