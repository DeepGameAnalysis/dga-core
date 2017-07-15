using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Export;
using Data.Gameobjects;
using Data.Gamestate;
using Clustering;
using Data.Utils;


namespace Detection
{
    /// <summary>
    /// Global accessed data storage for all relevant structures and objects 
    /// </summary>
    public class EncounterDetectionData
    {


        /// <summary>
        /// All currently active - not timed out - encounters
        /// </summary>
        public List<Encounter> OpenEncounters = new List<Encounter>();

        /// <summary>
        /// Timed out encounters
        /// </summary>
        public List<Encounter> ClosedEncounters = new List<Encounter>();

        /// <summary>
        /// Currently detected predecessors of a encounter
        /// </summary>
        public List<Encounter> Predecessors = new List<Encounter>();


        /// <summary>
        /// Allow to export data
        /// </summary>
        public bool ExportingEnabled = true;



        //
        // CONSTANTS TODO: read from file
        //

        //                          CSGO
        //
        // Timeouts in seconds.
        public const float TAU = 20;                                   // Time after which a encounter is not a predecessor anymore
        public const float ENCOUNTER_TIMEOUT = 20;                     // Time after which a Encounter ends if he is not extended before
        public const float WEAPONFIRE_VICTIMSEARCH_TIMEOUT = 5;        // Time after which a vicitim is not suitable for a weapon fire event
        public const float PLAYERHURT_WEAPONFIRESEARCH_TIMEOUT = 4;    // Time after which a a player hurt event is not suitable for a weapon fire event
        public const float PLAYERHURT_DAMAGEASSIST_TIMEOUT = 4;        // Time after which a player hurt event is not suitable for a damage assist




        //
        // Essentials
        //

        /// <summary>
        /// Tickrate of the demo this algorithm runs on in Hz. 
        /// </summary>
        public float Tickrate;

        /// <summary>
        /// Ticktime of the demo in ms. 
        /// </summary>
        public float Ticktime;

        /// <summary>
        /// All players - communicated by the meta-data - which are participating in this match. Get updated every tick.
        /// </summary>
        public Player[] Players;

        /// <summary>
        /// All entities controlled by players which are acting in this match. Get updated every tick.
        /// </summary>
        public HashSet<Entity> Entities;



        //
        // Data structures for saving vital information for further use in the encounter detection process
        //

        /// <summary>
        /// Holds every (attackerposition, victimposition) pair of a hitevent with the attackerposition as key
        /// </summary>
        public Hashtable Hit_hashtable = new Hashtable();

        /// <summary>
        /// Holds every (assister, assisted) pair of a playerdeath event with a assister
        /// </summary>
        public Hashtable DirectAssist_hashtable = new Hashtable();
        public Hashtable IndirectAssist_hashtable = new Hashtable();

        /// <summary>
        /// Holds every (assister, assisted) pair of a hurt events with assistance character
        /// </summary>
        public Hashtable DamageAssist_hashtable = new Hashtable();

        /// <summary>
        /// </summary>
        public Hashtable Heal_hashtable = new Hashtable();
        public Hashtable HealAssist_hashtable = new Hashtable();

        /// <summary>
        /// </summary>
        public Hashtable Resupply_hashtable = new Hashtable();


        //
        // Clustered spatial data
        //

        /// <summary>
        /// All clusters of attackpositions
        /// </summary>
        public EventPositionCluster[] PlayerHurt_clusters;





        //
        // Variables for building distance based links
        //
        /// <summary>
        /// Average of all eventbased supports
        /// </summary>
        public double SUPPORTRANGE_AVERAGE;

        /// <summary>
        /// Average of all eventbased combats
        /// </summary>
        public double ATTACKRANGE_AVERAGE;

        /// <summary>
        /// Average of all eventbased supports
        /// </summary>
        public double SUPPORTRANGE_MEDIAN;

        /// <summary>
        /// Average of all eventbased combats
        /// </summary>
        public double ATTACKRANGE_MEDIAN;



        //
        // Mapdata
        //

        /// <summary>
        /// Simple representation of the map to do basic sight calculations for players
        /// </summary>
        public Map Map;

        /// <summary>
        /// Name of the map this encounter detection is running on
        /// </summary>
        public string Mapname;

        /// <summary>
        /// Metadata about map. Important for calculations
        /// </summary>
        public MapMetaData Mapmeta;



        //
        // Matchdata
        //

        /// <summary>
        /// All data we have from this match.
        /// </summary>
        public Match Match;



        //
        // Efficiency and Prunning
        //

        /// <summary>
        /// Bit array for prunning of links - common use in RTS and MMOs
        /// </summary>
        public BitArray Links_pruningtable;

        /// <summary>
        /// Tickrate of the demo this algorithm runs on in Hz. 
        /// </summary>
        public float Tickrate_pruningrate;


        public Player[] GetPlayers()
        {
            return Players;
        }

        public List<Encounter> GetEncounters()
        {
            return ClosedEncounters;
        }
    }
}
