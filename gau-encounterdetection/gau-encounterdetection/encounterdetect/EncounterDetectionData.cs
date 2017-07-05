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
    public class EncounterDetectionData
    {
        /// <summary>
        /// Allow to export data
        /// </summary>
        private bool exportingEnabled = true;

        /// <summary>
        /// Exporter for csv file format
        /// </summary>
        private CSVExporter exporter = new CSVExporter();



        //
        // CONSTANTS TODO: read from file
        //

        //                          CSGO
        //
        // Timeouts in seconds.
        private const float TAU = 20;                                   // Time after which a encounter is not a predecessor anymore
        private const float ENCOUNTER_TIMEOUT = 20;                     // Time after which a Encounter ends if he is not extended before
        private const float WEAPONFIRE_VICTIMSEARCH_TIMEOUT = 5;        // Time after which a vicitim is not suitable for a weapon fire event
        private const float PLAYERHURT_WEAPONFIRESEARCH_TIMEOUT = 4;    // Time after which a a player hurt event is not suitable for a weapon fire event
        private const float PLAYERHURT_DAMAGEASSIST_TIMEOUT = 4;        // Time after which a player hurt event is not suitable for a damage assist




        //
        // Essentials
        //

        /// <summary>
        /// Tickrate of the demo this algorithm runs on in Hz. 
        /// </summary>
        public float tickrate;

        /// <summary>
        /// Ticktime of the demo in ms. 
        /// </summary>
        public float ticktime;

        /// <summary>
        /// All players - communicated by the meta-data - which are participating in this match. Get updated every tick.
        /// </summary>
        private Player[] players;

        /// <summary>
        /// All entities controlled by players which are acting in this match. Get updated every tick.
        /// </summary>
        private HashSet<Entity> entities;



        //
        // Data structures for saving vital information for further use in the encounter detection process
        //

        /// <summary>
        /// Holds every (attackerposition, victimposition) pair of a hitevent with the attackerposition as key
        /// </summary>
        public Hashtable hit_hashtable = new Hashtable();

        /// <summary>
        /// Holds every (assister, assisted) pair of a playerdeath event with a assister
        /// </summary>
        public Hashtable direct_assist_hashtable = new Hashtable();

        /// <summary>
        /// Holds every (assister, assisted) pair of a hurt events with assistance character
        /// </summary>
        public Hashtable damage_assist_hashtable = new Hashtable();

        /// <summary>
        /// </summary>
        public Hashtable heal_hashtable = new Hashtable();

        /// <summary>
        /// </summary>
        public Hashtable resupply_hashtable = new Hashtable();


        //
        // Clustered spatial data
        //

        /// <summary>
        /// All clusters of attackpositions
        /// </summary>
        public EventPositionCluster[] attacker_clusters;





        //
        // Variables for building links
        //

        /// <summary>
        /// Average of all eventbased supports
        /// </summary>
        private double SUPPORTRANGE_AVERAGE;

        /// <summary>
        /// Average of all eventbased combats
        /// </summary>
        private double ATTACKRANGE_AVERAGE;

        /// <summary>
        /// Average of all eventbased supports
        /// </summary>
        private double SUPPORTRANGE_MEDIAN;

        /// <summary>
        /// Average of all eventbased combats
        /// </summary>
        private double ATTACKRANGE_MEDIAN;



        //
        // Mapdata
        //

        /// <summary>
        /// Simple representation of the map to do basic sight calculations for players
        /// </summary>
        public Map map;

        /// <summary>
        /// Name of the map this encounter detection is running on
        /// </summary>
        public string mapname;

        /// <summary>
        /// Metadata about map. Important for calculations
        /// </summary>
        public MapMetaData mapmeta;



        //
        // Matchdata
        //

        /// <summary>
        /// All data we have from this match.
        /// </summary>
        private Match match;



        //
        // Efficiency and Prunning
        //

        /// <summary>
        /// Bit array for prunning of links - common use in RTS and MMOs
        /// </summary>
        public BitArray links_table;
    }
}
