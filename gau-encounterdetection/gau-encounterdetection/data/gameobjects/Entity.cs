using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;

namespace Data.Gameobjects
{
    public enum EntityType
    {
        RANGED,
        INFANTRY,
        CAVALERY,
        SIEGE
    }

    public class Entity
    {
        public string Entityname { get; set; }

        /// <summary>
        /// Unique identifier for this unit
        /// </summary>
        public long entity_id { get; set; }

        /// <summary>
        /// Player who controlls this unit
        /// </summary>
        public Player Owner { get; set; }

        /// <summary>
        /// Team of the player of this unit
        /// </summary>
        public Team Team { get { return Owner.GetTeam(); } }

        /// <summary>
        /// Defining a position of a player (if hes not controlling entities in the current game)
        /// Change in X means left or right movement on a minimap
        /// Change in Y means up or down movement on a minimap
        /// Change in Z means vertical movement (jumps, heightchanges on the map by climbing etc)
        /// </summary>
        public Point3D Position { get; set; }

        public Facing Facing { get; set; }

        public Velocity Velocity { get; set; }

        public int HP { get; set; }

        public float Attackrange { get; set; }

        public float Supportrange { get; set; }

        public float Damage { get; set; }
    }
}
