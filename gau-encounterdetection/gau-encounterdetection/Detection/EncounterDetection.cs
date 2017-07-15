using Clustering;
using CollisionManager;
using Data.Exceptions;
using Data.Gameevents;
using Data.Gameobjects;
using Data.Gamestate;
using Data.Utils;
using Export;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Functions;
using Preprocessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Detection
{

    public class EncounterDetection
    {

        private CSVExporter exporter = new CSVExporter();

        /// <summary>
        /// All data structures and tables and variables needed for encounter detection
        /// </summary>
        EncounterDetectionData EData = new EncounterDetectionData();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gamestate">The gamestate to work on</param>
        /// <param name="preprocessor">The preprocessor that should be used to prepare all necessary data</param>
        public EncounterDetection(ReplayGamestate gamestate, MapMetaData mapmeta, IPreprocessor preprocessor)
        {
            EData.Match = gamestate.match;
            EData.Mapname = gamestate.meta.mapname;
            EData.Mapmeta = mapmeta;
            EData.Tickrate = gamestate.meta.tickrate;
            EData.Ticktime = 1000 / EData.Tickrate;
            EData.Players = gamestate.meta.players.ToArray();

            Console.WriteLine("Match starts with " + EData.Players.Count() + " players.");
            PrintPlayers();

            // Gather and prepare data for later algorithms
            preprocessor.PreprocessData(gamestate, mapmeta, EData);

        }





        //
        //
        // MAIN ENCOUNTER DETECTION ALGORITHM
        //
        //
        #region MAIN ALGORITHM
        /// <summary>
        /// All currently active - not timed out - encounters
        /// </summary>
        private List<Encounter> open_encounters = new List<Encounter>();

        /// <summary>
        /// Timed out encounters
        /// </summary>
        private List<Encounter> closed_encounters = new List<Encounter>();

        /// <summary>
        /// Currently detected predecessors of a encounter
        /// </summary>
        private List<Encounter> predecessors = new List<Encounter>();



        //
        // Encounter Detection Stats - For later analysis. Export to CSV
        //
        #region Stats
        int tickCount = 0;
        int eventCount = 0;

        int killeventCount = 0;
        int hurteventCount = 0;

        int predecessorHandledCount = 0;
        int mergeEncounterCount = 0;
        int newEncounterCount = 0;
        int updateEncounterCount = 0;

        int damageAssistCount = 0;
        int killAssistCount = 0;
        int smokeAssistCount_sight = 0;
        int flashAssistCount_fov = 0;
        int flashAssistCount_sight = 0;

        int distancetestCLinksCount = 0;
        int distancetestSLinksCount = 0;
        int clustered_average_distancetestCLinksCount = 0;

        int sighttestCLinksCount = 0;
        int eventtestCLinksCount = 0;
        int eventestSightCLinkCount = 0;
        int spotteventsCount = 0;
        int isSpotted_playersCount = 0;
        int spotterFoundCount = 0;
        int noSpotterFoundCount = 0;

        int wfCount = 0;
        int wf_matchedVictimCount = 0;
        int wf_insertCount = 0;

        int flashexplodedCount = 0;
        int flashCLinkCount = 0;

        int no_clustered_distanceCount = 0;

        float totalencountertime = 0;
        float totalgametime = 0;

        int damageencountercount = 0;
        int killencountercount = 0;

        int totalencounterkillevents = 0;
        int totalencounterhurtevents = 0;
        int totalencounterspottedevents = 0;

        #endregion



        /// <summary>
        /// 
        /// </summary>
        public EncounterDetectionReplay DetectEncounters()
        {
            EncounterDetectionReplay replay = new EncounterDetectionReplay();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var round in EData.Match.rounds)
            {
                //Total gametime -> measure time each round is running
                totalgametime += (round.getRoundTickRange() * EData.Ticktime / 1000) * EData.Players.Length; // 10 = total playercount for csgo(if no one disconnected)

                foreach (var tick in new HashSet<Tick>(round.ticks))
                {
                    tickCount++;
                    eventCount += tick.getTickevents().Count;
                    CountEventsInTick(tick);
                    HandleServerEvents(tick); // Check if disconnects or reconnects happend in this tick

                    HandleBindings();

                    foreach (var updatedPlayer in tick.getUpdatedPlayers()) // Update tables if player is alive
                    {
                        UpdatePlayer(updatedPlayer);
                        if (updatedPlayer.IsSpotted) isSpotted_playersCount++;
                    }

                    HandleDisconnects();

                    CombatComponent component = BuildComponent(tick);

                    if (component == null) // No component in this tick
                        continue;

                    replay.InsertReplaydata(tick, component); // Save the tick with its component for later replaying. 

                    //
                    // Everything after here is just sorting components into encounters (use component.parent to identify to which encounter it belongs)
                    //
                    predecessors = SearchPredecessors(component); // Check if this component has predecessors

                    if (predecessors.Count == 0)
                    {
                        open_encounters.Add(new Encounter(component)); newEncounterCount++;
                    }

                    if (predecessors.Count == 1)
                    {
                        predecessors[0].update(component); updateEncounterCount++;
                    }

                    if (predecessors.Count > 1)
                    {
                        // Remove all predecessor encounters from open encounters because we re-add them as joint_encounter
                        open_encounters.RemoveAll(encounter => predecessors.Contains(encounter));
                        var joint_encounter = JoinWithPredecessors(predecessors); // Merge encounters holding these predecessors
                        joint_encounter.update(component);
                        open_encounters.Add(joint_encounter);
                        mergeEncounterCount++;
                    }

                    predecessors.Clear();

                    // Check encounter timeouts every tick
                    for (int i = open_encounters.Count - 1; i >= 0; i--)
                    {
                        Encounter e = open_encounters[i];
                        if (Math.Abs(e.getLatestTick() - tick.tick_id) * (EData.Ticktime / 1000) > EncounterDetectionData.ENCOUNTER_TIMEOUT)
                        {
                            open_encounters.Remove(e);
                            closed_encounters.Add(e);
                        }
                    }
                    // NEXT TICK

                } //NO TICKS LEFT -> Round has ended

                // Clear all Events and Queues at end of the round to prevent them from being carried into the next round
                ClearRoundData();
            }

            watch.Stop();
            var sec = watch.ElapsedMilliseconds / 1000.0f;

            // Clear match data
            EData.Match = null;

            //We are done. -> move open encounters to closed encounters
            closed_encounters.AddRange(open_encounters);
            open_encounters.Clear();

            // Calculate encounter stats

            foreach (var encounter in closed_encounters)
            {
                totalencountertime += (encounter.getTickRange() * EData.Ticktime / 1000) * encounter.getParticipatingPlayerCount();
                if (encounter.isDamageEncounter())
                    damageencountercount++;
                if (encounter.isKillEncounter())
                    killencountercount++;
                totalencounterkillevents += encounter.getEncounterKillEvents();
                totalencounterhurtevents += encounter.getEncounterHurtEvents();
                totalencounterspottedevents += encounter.getEncounterSpottedEvents();
            }


            // Dump stats to console
            #region Console ouput
            predecessorHandledCount = newEncounterCount + updateEncounterCount + mergeEncounterCount;
            Console.WriteLine("Hashed Hurt Events: " + EData.Hit_hashtable.Count);
            Console.WriteLine("Hashed Kill-Assist Events: " + EData.DirectAssist_hashtable.Count);
            Console.WriteLine("Hashed Damage-Assist Events: " + EData.DamageAssist_hashtable.Count);
            Console.WriteLine("\nComponent Predecessors handled: " + predecessorHandledCount);
            Console.WriteLine("New Encounters occured: " + newEncounterCount);
            Console.WriteLine("Encounter Merges occured: " + mergeEncounterCount);
            Console.WriteLine("Encounter Updates occured: " + updateEncounterCount);

            Console.WriteLine("\nWeaponfire-Events total: " + wfCount);
            Console.WriteLine("Total Weaponfire-Events matched: " + (wf_matchedVictimCount + wf_insertCount));

            Console.WriteLine("Weaponfire-Event first time matched victims: " + wf_matchedVictimCount);
            Console.WriteLine("Weaponfire-Events victims inserted into existing components: " + wf_insertCount);

            Console.WriteLine("\nSpotted-Events occured: " + spotteventsCount);
            Console.WriteLine("\nPlayer is spotted in: " + isSpotted_playersCount + " ticks");
            Console.WriteLine("No Spotters found in: " + noSpotterFoundCount + " ticks");
            Console.WriteLine("Spotters found in: " + spotterFoundCount + " ticks");

            Console.WriteLine("Sightbased Combatlinks: " + sighttestCLinksCount);
            Console.WriteLine("Sightbased Combatlinks Error: " + errorcount);
            Console.WriteLine("Distance (clustered) Combatlinks: " + clustered_average_distancetestCLinksCount);
            Console.WriteLine("Distance (averaged) Combatlinks: " + (distancetestCLinksCount + distancetestSLinksCount));

            Console.WriteLine("\nAssist-Supportlinks: " + killAssistCount);
            Console.WriteLine("DamageAssist-Supportlinks: " + damageAssistCount);
            Console.WriteLine("Nade-Supportlinks: ");
            Console.WriteLine("Smoke Supportlinks (sight): " + smokeAssistCount_sight);
            Console.WriteLine("Flashes exploded: " + flashexplodedCount);
            Console.WriteLine("Flash Supportlinks (sight): " + flashAssistCount_sight);
            Console.WriteLine("Flash Combatlinks: " + flashCLinkCount);


            Console.WriteLine("\n\n  Encounters found: " + closed_encounters.Count);

            Console.WriteLine("\n  Time to run Algorithm: " + sec + "sec. \n");
            #endregion
            //
            // Export data to csv
            //
            if (EData.ExportingEnabled)
                ExportEDDataToCSV(sec);

            return replay;
        }

        #endregion


        #region Methods for mainloop - connection handling
        /// <summary>
        /// All currently disconnected players
        /// </summary>
        private HashSet<Player> disconnectedplayers = new HashSet<Player>();

        /// <summary>
        /// All players that await binding
        /// </summary>
        private HashSet<Player> bindedplayers = new HashSet<Player>();

        /// <summary>
        /// Matches bot name to the ID of the player this bot is replaceing
        /// </summary>
        private Dictionary<string, long> botid_to_steamid = new Dictionary<string, long>();

        /// <summary>
        /// ID-Queue of disconnected players - just working if players who disconnect first rejoin first!
        /// </summary>
        private Queue<long> disconnected_ids = new Queue<long>();

        /// <summary>
        /// This method is checking for all connected/reconnecting players and bots
        /// </summary>
        private void HandleBindings()
        {
            foreach (var player in bindedplayers)
            {
                if (player.player_id == 0) // Player is a bot -> map his id on a disconnectedplayer -> we update the player with the botdata
                {
                    if (disconnected_ids.Count == 0) throw new PlayerBindingException();
                    botid_to_steamid.Add(player.Playername, disconnected_ids.Dequeue());
                    continue;
                }

                if (disconnectedplayers.Contains(player))
                    disconnectedplayers.Remove(player);
                else throw new PlayerBindingException(); // The player did not disconnect before. -> he missed the first round
            }
            bindedplayers.Clear();
        }

        /// <summary>
        /// This method is checking for all disconnecting players. If a player disconnects his actions or current links/effects(or if performed by a bot)
        /// are not observed any longer until he reconnects or a another real player joins instead
        /// </summary>
        private void HandleDisconnects()
        {
            foreach (var player in disconnectedplayers)
            {
                if (player.player_id == 0) // Player is a bot -> when a bot disconnects remove his binding to the players steamid
                {
                    botid_to_steamid.Remove(player.Playername);
                    continue;
                }
            }
        }

        /// <summary>
        /// Registeres players which have to be handled because of a connection problem
        /// </summary>
        /// <param name="tick"></param>
        private void HandleServerEvents(Tick tick)
        {
            foreach (var sevent in tick.getServerEvents())
            {
                Console.WriteLine(sevent.GameeventType + " " + sevent.Actor);

                var player = sevent.Actor;
                switch (sevent.GameeventType)
                {
                    case "player_bind":
                        bindedplayers.Add(player);

                        break;
                    case "player_disconnected":
                        disconnectedplayers.Add(player);
                        if (player.player_id != 0)
                            disconnected_ids.Enqueue(player.player_id);
                        break;
                    default: throw new Exception("Unkown ServerEvent");
                }
            }
        }
        #endregion


        #region Methods for mainloop - keep updates/data consistent


        private void CountEventsInTick(Tick tick)
        {
            foreach (var g in tick.getTickevents())
            {
                switch (g.GameeventType)
                {
                    case "player_hurt":
                        hurteventCount++;
                        break;
                    case "player_killed":
                        killeventCount++;
                        break;
                    case "weapon_fire":
                        wfCount++;
                        break;
                    case "player_spotted":
                        spotteventsCount++;
                        break;
                }
            }
        }

        /// <summary>
        /// Update a players with his most recent version. Further keeps track of all living players
        /// </summary>
        /// <param name="toUpdate"></param>
        private void UpdatePlayer(Player toUpdate)
        {
            int idcount = 0;

            foreach (var player in EData.Players)
            {
                var updateid = toUpdate.player_id;
                if (updateid == 0) // We want to update data from a bot in the name of a disconnected player
                    botid_to_steamid.TryGetValue(toUpdate.Playername, out updateid);

                if (player.player_id == updateid) // We found the player with a matching id -> update all changeable values
                {
                    idcount++;
                    if (toUpdate.IsDead()) // && !deadplayers.Contains(player)) //This player is dead but not in removed from the living -> do so
                    {
                        player.HP = toUpdate.HP;
                    }
                    else //Player is alive -> make sure hes in the living list and update him
                    {
                        player.Facing = toUpdate.Facing;
                        player.Position = toUpdate.Position;
                        player.Velocity = toUpdate.Velocity;
                        player.HP = toUpdate.HP;
                        player.IsSpotted = toUpdate.IsSpotted;
                    }
                }

                if (idcount > 1)
                {
                    PrintPlayers();
                    throw new Exception("More than one player with id living or revive is invalid: " + toUpdate.player_id);
                }
            }

            if (idcount == 0) throw new Exception("No player with id: " + toUpdate.player_id + " found.");

        }

        #endregion


        #region Methods for Encounterdefinition and Componentmanipulation
        /// <summary>
        /// Searches all predecessor encounters of an component. or in other words:
        /// tests if a component is a successor of another encounters component
        /// </summary>
        /// <param name="newcomp"></param>
        /// <returns></returns>
        private List<Encounter> SearchPredecessors(CombatComponent newcomp)
        {
            List<Encounter> predecessors = new List<Encounter>();
            foreach (var encounter in open_encounters.Where(e => CheckPredecessorTimeout(e.tick_id, newcomp.tick_id, EncounterDetectionData.TAU)))
            {
                bool registered = false;
                foreach (var comp in encounter.cs)
                {
                    // Test if c and comp have at least two players in common -> Intersection of player lists
                    var intersectPlayers = comp.players.Intersect(newcomp.players).ToList();

                    if (intersectPlayers.Count < 2)
                        continue;
                    //are these players from different teams
                    var knownteam = intersectPlayers[0].GetTeam(); //TODO: Shorten
                    foreach (var p in intersectPlayers)
                    {
                        // Team different to one we know -> this encounter e is a predecessor of the component comp
                        if (knownteam != Team.None && knownteam != p.GetTeam())
                        {
                            predecessors.Add(encounter);
                            registered = true; // Stop multiple adding of encounter
                            break;
                        }
                    }
                    if (registered) break;
                }
            }

            return predecessors;
        }

        private bool CheckPredecessorTimeout(int eid, int tickid, float TAU)
        {
            var dt = tickid - eid;
            if (dt < 0) throw new Exception("Encounter cannot be newer than component");
            if (dt * (EData.Ticktime / 1000) <= TAU)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Joins a list of encounters into one single encounter (Merge-case).
        /// </summary>
        /// <param name="predecessors"></param>
        /// <returns></returns>
        private Encounter JoinWithPredecessors(List<Encounter> predecessors)
        {
            List<CombatComponent> cs = new List<CombatComponent>();
            foreach (var encounter in predecessors)
            {
                cs.AddRange(encounter.cs); // Watch for OutOfMemoryExceptions here if too many predecessors add up -> high tau -> one big encounter!! 
            }
            
            var merged_encounter = new Encounter(cs);
            merged_encounter.cs.ForEach(comp => comp.parent = merged_encounter); // Set new parent encounter for all components
            return merged_encounter;
        }

        /// <summary>
        /// Insert the given link into the component at the given tick_id
        /// </summary>
        /// <param name="tick_id"></param>
        /// <param name="insertlink"></param>
        private void InsertLinkIntoComponent(int tick_id, Link insertlink)
        {
            foreach (var en in open_encounters) // Search the component in a encounter in which this link has to be sorted in 
            {
                bool inserted = false;
                var valid_comps = en.cs.Where(comp => comp.tick_id == tick_id); // Get the component with this tickid
                if (valid_comps.Count() > 1)
                    throw new Exception("More than one component at tick :" + tick_id + " existing. Components have to be unique!");
                else if (valid_comps.Count() == 0)
                    continue;

                valid_comps.First().links.Add(insertlink);
                wf_insertCount++;
                inserted = true;

                if (inserted) //This should be useless if components and their tick_ids are unique
                    break;
            }
        }
        #endregion

        /// <summary>
        /// Main Method to build components in CS:GO
        /// Feeds the component with a links resulting from the procedure handling this tick @TODO implement linker here
        /// </summary>
        /// <param name="component"></param>
        /// <param name="g"></param>
        private CombatComponent BuildComponent(Tick tick)
        {
            List<Link> links = new List<Link>();

            //searchEventbasedSightCombatLinks(tick, links);
            SearchSightbasedSightCombatLinks(tick, links); //First update playerlevels

            //searchClusterDistancebasedLinks(links); // With clusterbased distance
            //searchDistancebasedLinks(links); // With average distance 

            //searchEventbasedLinks(tick, links);
            //searchEventbasedNadeSupportlinks(tick, links);

            if (links.Count != 0) //If links have been found
            {
                CombatComponent combcomp = new CombatComponent();
                combcomp.tick_id = tick.tick_id;
                links.RemoveAll(link => link == null); //If illegal links have been built they are null -> remove them
                combcomp.links = links;
                combcomp.assignPlayers();
                combcomp.assignComponentEventcount(tick, EData.Players);

                return combcomp;
            }

            return null;

        }


        #region Methods to find Encounters in CS:GO

        /// <summary>
        /// Queue of all hurtevents(HE) that where fired. Use these to search for a coressponding weaponfire event.
        /// Value is the tick_id as int where the event happend.
        /// </summary>
        private Dictionary<PlayerHurt, int> registeredHEQueue = new Dictionary<PlayerHurt, int>();


        /// <summary>
        /// Weaponfire events(WFE) that are waiting for their check.
        /// Value is the tick_id as int where the event happend.
        /// </summary>
        private Dictionary<WeaponFire, int> pendingWFEQueue = new Dictionary<WeaponFire, int>();


        /// <summary>
        /// Active nades such as smoke and fire nades which have not ended and need to be tested every tick they are effective
        /// Value is the tick_id as int where the event happend.
        /// </summary>
        private Dictionary<NadeEvents, int> activeNades = new Dictionary<NadeEvents, int>();


        /// <summary>
        /// Current victimcandidates
        /// </summary>
        private List<Player> vcandidates = new List<Player>();


        /// <summary>
        /// Current spottercandidates
        /// </summary>
        private List<Player> scandidates = new List<Player>();

        /// <summary>
        /// Search all potential combatlinks based on sight using a spotted variable from the replay data(equivalent to DOTA2 version: player is in attackrange)
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="links"></param>
        private void SearchEventbasedSightCombatLinks(Tick tick, List<Link> links)
        {
            foreach (var uplayer in EData.Players.Where(p => !p.IsDead() && p.IsSpotted)) // Search for all spotted players in this tick who spotted them
            {
                var potential_spotter = SearchSpotterCandidates(uplayer);
                // This should not happend because spotted table is correct and somebody must have seen the player!!
                if (potential_spotter == null)
                {
                    noSpotterFoundCount++;
                    continue;
                }

                links.Add(new Link(potential_spotter, uplayer, LinkType.COMBATLINK, Direction.DEFAULT));
                eventestSightCLinkCount++;
                spotterFoundCount++;
            }

        }


        /// <summary>
        /// Search all combatlinks that are based on pure sight. No events are used here. Just positional data and line of sight
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="links"></param>
        private void SearchSightbasedSightCombatLinks(Tick tick, List<Link> links)
        {
            // Update playerlevels before we start using them to search links
            foreach (var p in EData.Players.Where(counterplayer => !counterplayer.IsDead()))
            {
                if (EData.Map.Levels.ContainsKey(p.player_id))
                    EData.Map.Levels[p.player_id] = EData.Map.FindLevelFromPlayer(p);
                else
                    EData.Map.Levels.Add(p.player_id, EData.Map.FindLevelFromPlayer(p));
            }

            // Check for each team if a player can see a player of the other team
            foreach (var player in EData.Players.Where(player => !player.IsDead() && player.GetTeam() == Team.CT))
            {
                foreach (var counterplayer in EData.Players.Where(counterplayer => !counterplayer.IsDead() && counterplayer.GetTeam() != Team.CT))
                {
                    CheckVisibilityBetween(player, counterplayer, links);
                }
            }
        }


        private static int errorcount = 0;
        /// <summary>
        /// Checks if p1 can see p2 considering obstacles between them: !! this method can only be used when playerlevels get updated see sightbasedsightcombatlinks
        /// </summary>
        /// <param name="links"></param>
        /// <param name="playerlevels"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private bool CheckVisibilityBetween(Player p1, Player p2, List<Link> links)
        {
            // Console.WriteLine("New test");
            bool p1FOVp2 = FOVFunctions.IsInHVFOV(p1.Position, p1.Facing.Yaw, p2.Position, 106.0f); // p2 is in fov of p1
            bool p2FOVp1 = FOVFunctions.IsInHVFOV(p2.Position, p2.Facing.Yaw, p1.Position, 106.0f); // p2 is in fov of p1
            if (!p1FOVp2 && !p2FOVp1) return false; // If both false -> no sight from p1 to p2 possible because p2 is not even in the fov of p1  and vice versa -> no links

            //Level height of p1 and p2
            var p1Height = EData.Map.Levels[p1.player_id].HeightIndex;
            var p2Height = EData.Map.Levels[p2.player_id].HeightIndex;

            var current_maplevel = EData.Map.Levels[p1.player_id];

            var p1PLOSp2 = CollisionController.PLOSIntersectsObstacle2D(p1.Position, p2.Position, current_maplevel); // Check if the p1`s view is blocked on his level
            var p2PLOSp1 = CollisionController.PLOSIntersectsObstacle2D(p2.Position, p1.Position, current_maplevel); // Check if the p1`s view is blocked on his level
            //if(coll_pos == null)Console.WriteLine("Start coll: " + coll_pos);
            if (p1Height != p2Height) throw new Exception("Wrong level height. Not possible");
            //If p2 was in FOV of p1 -> check if a collision with obstacle occured
            if (p1FOVp2)
            {
                if (!p1PLOSp2.HasValue)
                {
                    //links.Add(new Link(p1, p2, LinkType.COMBATLINK, Direction.DEFAULT)); sighttestCLinksCount++;
                }
                else
                    //links.Add(new Link(p1, p2, LinkType.COMBATLINK, Direction.DEFAULT, p1PLOSp2)); // Just for drawing collisions
                return false;
            }

            if (p2FOVp1)
            {
                if (!p2PLOSp1.HasValue)
                {
                    //links.Add(new Link(p2, p1, LinkType.COMBATLINK, Direction.DEFAULT)); sighttestCLinksCount++;
                }
                else
                    //links.Add(new Link(p2, p1, LinkType.COMBATLINK, Direction.DEFAULT, p2PLOSp1)); // Just for drawing collisions
                return false;
            }


            return false;

        }

        /// <summary>
        /// Algorithm searching links simply on euclid distance
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="links"></param>
        private void SearchDistancebasedLinks(List<Link> links)
        {
            foreach (var player in EData.Players.Where(p => !p.IsDead()))
            {
                foreach (var other in EData.Players.Where(p => !p.Equals(player) && !p.IsDead()))
                {
                    var distance = DistanceFunctions.GetEuclidDistance3D(player.Position, other.Position);

                    if (distance <= EData.ATTACKRANGE_AVERAGE && other.GetTeam() != player.GetTeam())
                    {
                        links.Add(new Link(player, other, LinkType.COMBATLINK, Direction.DEFAULT));
                        distancetestCLinksCount++;
                    }/*
                    else if (distance <= SUPPORTRANGE_AVERAGE && other.getTeam() == player.getTeam())
                    {
                        links.Add(new Link(player, other, LinkType.SUPPORTLINK, Direction.DEFAULT));
                        distancetestSLinksCount++;
                    }*/
                }
            }
        }

        /// <summary>
        /// Algorithm searching links by an attackrange provided by average-attackrange of a cluster
        /// </summary>
        /// <param name="links"></param>
        private void SearchClusterDistancebasedLinks(List<Link> links)
        {
            foreach (var player in EData.Players.Where(p => !p.IsDead()))
            {
                foreach (var other in EData.Players.Where(p => !p.Equals(player) && !p.IsDead()))
                {
                    var distance = DistanceFunctions.GetEuclidDistance3D(player.Position, other.Position);
                    EventPositionCluster playercluster = null;
                    for (int i = 0; i < EData.PlayerHurt_clusters.Length; i++) // TODO: Change this if clustercount gets to high. Very slow
                    {
                        var cluster = EData.PlayerHurt_clusters[i];
                        if (cluster.Boundings.Contains(player.Position.SubstractZ()))
                        {
                            playercluster = cluster;
                            break;
                        }
                    }
                    if (playercluster == null)
                    {
                        no_clustered_distanceCount++;
                        continue; // No Cluster found
                    }

                    var attackrange = playercluster.cluster_range_average;
                    if (distance <= attackrange && other.GetTeam() != player.GetTeam())
                    {
                        links.Add(new Link(player, other, LinkType.COMBATLINK, Direction.DEFAULT));
                        clustered_average_distancetestCLinksCount++;
                    }
                    // NO SUPPORTLINKS POSSIBLE WITH THIS METHOD BECAUSE NO SUPPORT CLUSTERS EXISTING
                }
            }
        }


        /// <summary>
        /// Algorithm searching links based on CS:GO Replay Events
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="links"></param>
        private void SearchEventbasedLinks(Tick tick, List<Link> links)
        {
            // Events contain players with newest data to this tick
            foreach (var g in tick.getTickevents())
            { // Read all gameevents in that tick and build links of them for the component
                switch (g.GameeventType)
                {
                    //
                    //  Combatlink-Relevant Events
                    //
                    case "player_hurt":
                        PlayerHurt ph = (PlayerHurt)g;
                        if (ph.Actor.GetTeam() == ph.Victim.GetTeam()) continue; // No Team damage
                        var link_ph = new Link(ph.Actor, ph.Victim, LinkType.COMBATLINK, Direction.DEFAULT);
                        links.Add(link_ph);
                        link_ph.Impact = ph.HP_damage + ph.Armor_damage;
                        HandleIncomingHurtEvent(ph, tick.tick_id, links); // CAN PRODUCE SUPPORTLINKS!
                        eventtestCLinksCount++;
                        break;
                    case "player_killed":
                        PlayerKilled pk = (PlayerKilled)g;
                        if (pk.Actor.GetTeam() == pk.Victim.GetTeam()) continue; // No Team kills
                        var link_pk = new Link(pk.Actor, pk.Victim, LinkType.COMBATLINK, Direction.DEFAULT);
                        link_pk.Impact = pk.HP_damage + pk.Armor_damage;
                        link_pk.IsKill = true;
                        links.Add(link_pk);

                        if (pk.Assister != null && pk.Assister.GetTeam() == pk.Actor.GetTeam())
                        {
                            //links.Add(new Link(pk.assister, pk.actor, LinkType.SUPPORTLINK, Direction.DEFAULT));
                            //killAssistCount++;
                        }
                        eventtestCLinksCount++;

                        break;
                    case "weapon_fire":
                        WeaponFire wf = (WeaponFire)g;
                        var potential_victim = SearchVictimCandidate(wf, tick.tick_id);

                        // No candidate found. Either wait for a incoming playerhurt event or there was no suitable victim
                        if (potential_victim == null) break;
                        if (wf.Actor.GetTeam() != potential_victim.GetTeam()) break;
                        wf_matchedVictimCount++;
                        links.Add(new Link(wf.Actor, potential_victim, LinkType.COMBATLINK, Direction.DEFAULT));
                        eventtestCLinksCount++;

                        break;
                }
            }
        }


        /// <summary>
        /// When a hurtevent is registered we want to test if some of our pending weaponfire events match this playerhurt event.
        /// If so we have to insert the link that arises into the right Combatcomponent.
        /// </summary>
        /// <param name="ph"></param>
        /// <param name="tick_id"></param>
        private void HandleIncomingHurtEvent(PlayerHurt ph, int tick_id, List<Link> links)
        {
            for (int index = registeredHEQueue.Count - 1; index >= 0; index--)
            {
                var item = registeredHEQueue.ElementAt(index);
                var hurtevent = item.Key;
                var htick_id = item.Value;
                int tick_dt = Math.Abs(htick_id - tick_id);

                if (tick_dt * (EData.Ticktime / 1000) > EncounterDetectionData.PLAYERHURT_DAMAGEASSIST_TIMEOUT)
                {
                    registeredHEQueue.Remove(hurtevent); // Check timeout
                    continue;
                }

                // If same victim but different actors from the same team-> damageassist -> multiple teammates attack one enemy
                if (ph.Victim.Equals(hurtevent.Victim) && !ph.Actor.Equals(hurtevent.Actor) && ph.Actor.GetTeam() == hurtevent.Actor.GetTeam())
                {
                    links.Add(new Link(ph.Actor, hurtevent.Actor, LinkType.SUPPORTLINK, Direction.DEFAULT));
                    if (!EData.DamageAssist_hashtable.ContainsKey(ph.Actor.Position)) EData.DamageAssist_hashtable.Add(ph.Actor.Position, hurtevent.Actor.Position);
                    damageAssistCount++;
                }
                // If ph.actor hits an enemy while this enemy has hit somebody from p.actors team
                if (ph.Victim.Equals(hurtevent.Actor) && hurtevent.Victim.GetTeam() == ph.Actor.GetTeam())
                {
                    links.Add(new Link(ph.Actor, hurtevent.Victim, LinkType.SUPPORTLINK, Direction.DEFAULT));
                    if (!EData.DirectAssist_hashtable.ContainsKey(ph.Actor.Position)) EData.DirectAssist_hashtable.Add(ph.Actor.Position, hurtevent.Victim.Position);
                    damageAssistCount++;
                }
            }

            registeredHEQueue.Add(ph, tick_id);

            for (int index = pendingWFEQueue.Count - 1; index >= 0; index--)
            {
                var item = pendingWFEQueue.ElementAt(index);
                var weaponfireevent = item.Key;
                var wftick_id = item.Value;

                int tick_dt = Math.Abs(wftick_id - tick_id);
                if (tick_dt * (EData.Ticktime / 1000) > EncounterDetectionData.PLAYERHURT_WEAPONFIRESEARCH_TIMEOUT)
                {
                    pendingWFEQueue.Remove(weaponfireevent); //Check timeout
                    continue;
                }

                if (ph.Actor.Equals(weaponfireevent.Actor) && !ph.Actor.IsDead() && EData.Players.Where(p => !p.IsDead()).Contains(weaponfireevent.Actor)) // We found a weaponfire event that matches the new playerhurt event
                {
                    Link insertlink = new Link(weaponfireevent.Actor, ph.Victim, LinkType.COMBATLINK, Direction.DEFAULT);
                    eventtestCLinksCount++;

                    InsertLinkIntoComponent(wftick_id, insertlink);
                    pendingWFEQueue.Remove(weaponfireevent); // Delete the weaponfire event from the queue
                }

            }
        }



        private void SearchEventbasedNadeSupportlinks(Tick tick, List<Link> links)
        {
            // Update active nades list with the new tick
            foreach (var g in tick.getTickevents())
            {
                switch (g.GameeventType)
                {
                    //    
                    //  Supportlink-Relevant Events
                    //
                    case "flash_exploded":
                        FlashNade flash = (FlashNade)g;
                        if (flash.Flashedplayers.Count == 0)
                            continue; // The nade flashed noone
                        activeNades.Add(flash, tick.tick_id);
                        flashexplodedCount++;

                        break;
                    case "firenade_exploded":
                    case "decoy_exploded":
                    case "smoke_exploded":
                        NadeEvents timedNadeStart = (NadeEvents)g;
                        activeNades.Add(timedNadeStart, tick.tick_id);
                        break;
                    case "smoke_ended":
                    case "firenade_ended":
                    case "decoy_ended":
                        NadeEvents timedNadeEnd = (NadeEvents)g;
                        activeNades.Remove(timedNadeEnd);

                        break;
                    default:
                        break;
                }
            }

            UpdateFlashes(tick); // Flashes dont provide an end-event so we have to figure out when their effect has ended -> we update their effecttime
            SearchSupportFlashes(links);

            SearchSupportSmokes(links);

        }


        /// <summary>
        /// Updates all active flashes. If within a flash is no player which has flashtime(time this player is flashed - flashduration in data) left. The flash has ended.
        /// </summary>
        /// <param name="tick"></param>
        private void UpdateFlashes(Tick tick)
        {
            foreach (var flashitem in activeNades.Where(item => item.Key.GameeventType == "flash_exploded").ToList()) // Make Copy to enable deleting while iterating
            {
                int finishedcount = 0;
                FlashNade flash = (FlashNade)flashitem.Key;
                int ftick = flashitem.Value;
                float tickdt = Math.Abs(ftick - tick.tick_id);
                foreach (var player in flash.Flashedplayers)
                {
                    if (player.Flashedduration >= 0)
                    {
                        float dtime = tickdt * (EData.Ticktime / 1000);
                        player.Flashedduration -= dtime; // Count down time
                    }
                    else
                        finishedcount++;
                }
                if (finishedcount == flash.Flashedplayers.Count)
                    activeNades.Remove(flash);
            }
        }

        /// <summary>
        /// Searches Supportlinks built by flashbang events
        /// </summary>
        /// <param name="links"></param>
        private void SearchSupportFlashes(List<Link> links)
        {
            foreach (var f in activeNades.Where(item => item.Key.GameeventType == "flash_exploded")) //Update players flashtime and check for links
            {
                FlashNade flash = (FlashNade)f.Key;

                // Each (STILL!) living flashed player - as long as it is not a teammate of the actor - is tested for sight on a teammember of the flasher (has flasher prevented sight on one of his teammates) 
                var flashedenemies = flash.Flashedplayers.Where(player => player.GetTeam() != flash.Actor.GetTeam() && player.Flashedduration >= 0 && GetLivingPlayer(player) != null);
                if (flashedenemies.Count() == 0)
                    continue;

                foreach (var flashedEnemyplayer in flashedenemies)
                {

                    links.Add(new Link(flash.Actor, flashedEnemyplayer, LinkType.COMBATLINK, Direction.DEFAULT)); //Sucessful flash counts as combatlink
                    eventtestCLinksCount++;
                    flashCLinkCount++;

                    foreach (var teammate in EData.Players.Where(teamate => !teamate.IsDead() && teamate.GetTeam() == flash.Actor.GetTeam() && flash.Actor != teamate))
                    {
                        if (CheckVisibilityBetween(GetLivingPlayer(flashedEnemyplayer), teammate, null))
                        {
                            Link flashsupportlink = new Link(flash.Actor, teammate, LinkType.SUPPORTLINK, Direction.DEFAULT);
                            links.Add(flashsupportlink);
                            flashAssistCount_sight++;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Searches supportlinks built by smokegrenades
        /// </summary>
        /// <param name="supportlinks"></param>
        private void SearchSupportSmokes(List<Link> supportlinks)
        {
            foreach (var smokeitem in activeNades.Where(item => item.Key.GameeventType == "smoke_exploded"))
            {
                foreach (var counterplayer in EData.Players.Where(player => !player.IsDead() && player.GetTeam() != smokeitem.Key.Actor.GetTeam()))
                {
                    //If a player from the opposing team of the smoke thrower saw into the smoke
                    var nadecircle = new Circle2D(new Point2D(smokeitem.Key.position.X, smokeitem.Key.position.Y), 250);
                    if (nadecircle.IntersectsVector2D(counterplayer.Position.SubstractZ(), counterplayer.Facing.Yaw))
                    {
                        // Check if he could have seen a player from the thrower team
                        foreach (var teammate in EData.Players.Where(teammate => !teammate.IsDead() && teammate.GetTeam() == smokeitem.Key.Actor.GetTeam()))
                        {
                            if (CheckVisibilityBetween(counterplayer, teammate, null))
                            {
                                // The actor supported a teammate -> Supportlink
                                Link smokesupportlink = new Link(smokeitem.Key.Actor, teammate, LinkType.SUPPORTLINK, Direction.DEFAULT);
                                supportlinks.Add(smokesupportlink);
                                smokeAssistCount_sight++;
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Searches the player that has most probable attacked another player with the given weapon fire event
        /// This method takes weaponfire events into account which came after a playerhurt event of the weaponfire event actor.
        /// And in most cases a player fires and misses therefore theres a long time between when he might hit the seen opponent because he hides. But still he saw and shot at him. These events are lost here
        /// </summary>
        /// <param name="wf"></param>
        /// <returns></returns>
        private Player SearchVictimCandidate(WeaponFire wf, int tick_id)
        {

            for (int index = registeredHEQueue.Count - 1; index >= 0; index--)
            {
                var item = registeredHEQueue.ElementAt(index);
                var hurtevent = item.Key;
                var htick_id = item.Value;

                int tick_dt = Math.Abs(htick_id - tick_id);
                if (tick_dt * (EData.Ticktime / 1000) > EncounterDetectionData.WEAPONFIRE_VICTIMSEARCH_TIMEOUT) // 20 second timeout for hurt events
                {
                    registeredHEQueue.Remove(hurtevent);
                    continue;
                }
                // If we find a actor that hurt somebody. this weaponfireevent is likely to be a part of his burst and is therefore a combatlink
                var aliveplayers = EData.Players.Where(player => !player.IsDead());
                if (wf.Actor.Equals(hurtevent.Actor) && hurtevent.Victim.GetTeam() != wf.Actor.GetTeam() && aliveplayers.Contains(hurtevent.Victim) && aliveplayers.Contains(wf.Actor))
                {
                    // Roughly test if an enemy can see our actor
                    if (FOVFunctions.IsInHVFOV(wf.Actor.Position, wf.Actor.Facing.Yaw, hurtevent.Victim.Position, 106.0f) && hurtevent.Victim.IsSpotted)
                    {
                        vcandidates.Add(hurtevent.Victim);
                        // Order by closest distance or by closest los player to determine which is the probablest candidate
                        //vcandidates.OrderBy(candidate => EDMathLibrary.getEuclidDistance2D(hvictimpos, wfactorpos));
                        vcandidates.OrderBy(candidate => FOVFunctions.GetLOSOffset2D(wf.Actor.Position.SubstractZ(), wf.Actor.Facing.Yaw, hurtevent.Victim.Position.SubstractZ())); //  Offset = Angle between lineofsight of actor and position of candidate
                        break;
                    }

                }
                else // We didnt find a matching hurtevent but there is still a chance for a later hurt event to suite for wf -> store this event and try another time
                {
                    pendingWFEQueue.Add(wf, tick_id);
                    break;
                }
            }

            if (vcandidates.Count == 0)
                return null;
            // Choose the first in the list as we ordered it by Offset (see above)
            var victim = vcandidates[0];
            if (victim.GetTeam() == wf.Actor.GetTeam()) throw new Exception("No teamfire possible for combatlink creation");
            vcandidates.Clear();
            return victim;
        }


        /// <summary>
        /// Searches players who possibly have spotted a certain player
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        private Player SearchSpotterCandidates(Player actor)
        {
            if (actor.IsDead()) return null;

            foreach (var counterplayer in EData.Players.Where(player => !player.IsDead() && player.GetTeam() != actor.GetTeam()))
            {
                // Test if an enemy can see our actor
                if (FOVFunctions.IsInHVFOV(counterplayer.Position, counterplayer.Facing.Yaw, actor.Position, 106.0f))
                {
                    scandidates.Add(counterplayer);
                    scandidates.OrderBy(candidate => FOVFunctions.GetLOSOffset2D(counterplayer.Position.SubstractZ(), counterplayer.Facing.Yaw, actor.Position.SubstractZ())); //  Offset = Angle between lineofsight of actor and position of candidate
                }
            }

            if (scandidates.Count == 0)
                return null;

            var nearestplayer = scandidates[0];
            if (nearestplayer.GetTeam() == actor.GetTeam()) throw new Exception("No teamspotting possible");
            scandidates.Clear();
            return nearestplayer;
        }

        #endregion




        //
        //
        // HELPING METHODS AND ID HANDLING
        //
        //
        #region Helping Methods
        private bool rowset = false;

        private void ExportEDDataToCSV(float sec)
        {
            //TODO: check this earlier!
            if (totalencounterhurtevents == 0 || totalencounterkillevents == 0)
                throw new Exception("Replay useless because essential events were not saved");
            if (!rowset)
            {
                exporter.AddRow();
                rowset = true;
            }
            exporter["Map"] = EData.Mapname;
            exporter["Tickrate"] = EData.Tickrate;
            exporter["Gametime"] = totalgametime;
            exporter["Encountertime"] = totalencountertime;
            var qe = totalencountertime / (double)totalgametime;
            exporter["Anteil Encountertime and Gametime"] = qe;
            exporter["Runtime in sec"] = sec;
            exporter["Observed ticks"] = tickCount;
            exporter["Observed events"] = eventCount;
            exporter["Hurt-Events"] = hurteventCount;
            exporter["Killed-Events"] = killeventCount;
            exporter["Players spotted in ticks"] = isSpotted_playersCount;
            exporter["Encounters found"] = closed_encounters.Count;
            exporter["Damage Encounters"] = damageencountercount;
            exporter["Kill Encounters"] = killencountercount;
            exporter["Hurtevents in Encounters"] = totalencounterhurtevents;
            exporter["Killevents in Encounters"] = totalencounterkillevents;
            exporter["Spotted in Encounters"] = totalencounterspottedevents;
            var qh = totalencounterhurtevents / (double)hurteventCount;
            var qk = totalencounterkillevents / (double)killeventCount;
            var qs = totalencounterspottedevents / (double)isSpotted_playersCount;
            exporter["Hurtevents in Encounters / Hurt-Events"] = qh;
            exporter["Killevents in Encounters / Killed-Events"] = qk;
            exporter["Spotted in Encounters / Players spotted in ticks"] = qs;


            exporter["Sightcombatlink - Sightbased"] = sighttestCLinksCount;
            exporter["Sightcombatlink - Eventbased"] = eventestSightCLinkCount;
            exporter["Combatlinks - Eventbased"] = eventtestCLinksCount;
            exporter["Combatlinks - Distancebased(Average Hurtrange)"] = distancetestCLinksCount;
            exporter["Combatlinks - Distancebased(Clustered Range)"] = clustered_average_distancetestCLinksCount;
            exporter["Supportlinks - Eventbased"] = damageAssistCount + killAssistCount;
            exporter["Supportlinks - Distancebased(Average Hurtrange)"] = distancetestSLinksCount;
            exporter["Supportlinks - Smoke"] = smokeAssistCount_sight;
            exporter["Supportlinks - Flash"] = flashAssistCount_sight;
            exporter["Supportlinks - Assist"] = killAssistCount;
            exporter["Supportlinks - Damageassist"] = damageAssistCount;
            exporter.ExportToFile("encounter_detection_results.csv");
        }


        /// <summary>
        /// Clear all lists and queues that loose relevance at the end of the round to prevent events from carrying over to the next round
        /// </summary>
        private void ClearRoundData()
        {
            activeNades.Clear();
            registeredHEQueue.Clear();
            pendingWFEQueue.Clear();
            scandidates.Clear();
            vcandidates.Clear();
        }

        /// <summary>
        /// Gets player that is not updated and tell if he is dead.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Player GetLivingPlayer(Player p)
        {
            var players = this.EData.Players.Where(player => player.player_id == p.player_id);

            if (players.Count() == 1)
                return players.First();
            else
                return null;
        }

        private void PrintPlayers()
        {
            foreach (var p in EData.Players)
                Console.WriteLine(p);
        }

        public Player[] GetPlayers()
        {
            return EData.Players;
        }

        public List<Encounter> GetEncounters()
        {
            return closed_encounters;
        }

        /*// Check for tunnels
            var p2coll_pos = EDMathLibrary.LOSIntersectsMap(p2.position, p1.position, playerlevels[p2.player_id]); // Check if the p2`s view is blocked on his level
            if (p2coll_pos == null && coll_pos == null && p1Height != p2Height) // Both players on different levels claim to have free sight on the other one but no level transition was registered -> p1 or p2 is in a tunnel
                return null;

            //
            // Case: p1 and p2 stand on different levels and p2 is in the FOV of p1
            //
            //Error occuring -> a gridcell registers two points with different level assigning -> the cell is free on both level -> collision can be null although player is standing in a differnt level
            if (coll_pos == null && p1Height != p2Height) { errorcount++; return null; }

            if (coll_pos == null) throw new Exception("Shit cannot happen");

            // All levels that have to see from p1 to p2 -> p1`s LOS clips these levels if he wants to see him
            MapLevel[] clipped_levels = map.getClippedLevels(p1Height, p2Height);

            for (int i = 0; i < clipped_levels.Length; i++) // Check next levels: p1Height+1, p1Height+2
            {
                if (coll_pos == null) // No collision -> check next level with same line
                    throw new Exception("No null collision alloweed for further testing. Tunnel must have occured");

                var nextlevel = clipped_levels[i];

                if (coll_pos != null) // collision ->  check if a new level is beginning or if there is wall
                {
                    EDVector3D last_coll_pos = coll_pos;
                    coll_pos = EDMathLibrary.LOSIntersectsMap(last_coll_pos, p2.position, nextlevel); // New line from last collision
                    // Free sight before level of p2 was entered through transition -> tunnel
                    if (coll_pos == null && nextlevel.height != p2Height) return null;
                    // Free sight on the last level -> sight was free
                    if (coll_pos == null && nextlevel.height == p2Height) continue;

                    // If a collision was found in the new level which is not our startpoint from the new line -> Transition between levels -> search next level
                    if (coll_pos != null && !coll_pos.Equals(last_coll_pos))
                        continue;
                    // If a collision was found in the new level which is equals to our startpoint from the new line -> Next level has obstacle at same position -> No free sight
                    else if (coll_pos != null && coll_pos.Equals(last_coll_pos))
                        return null; // Obstacle found -> abort link search
                }
            }

            // Sight has been free from p1 to p2 so add a combatlink
            sighttestCLinksCount++;
            return new Link(p1, p2, LinkType.COMBATLINK, Direction.DEFAULT);
         */
        #endregion
    }
}
