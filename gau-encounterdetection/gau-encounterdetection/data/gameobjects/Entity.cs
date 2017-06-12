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
        public string entityname { get; set; }

        /// <summary>
        /// Unique identifier for this unit
        /// </summary>
        public long entity_id { get; set; }

        /// <summary>
        /// Player who controlls this unit
        /// </summary>
        public Player  owner { get; set; }

        /// <summary>
        /// Team of the player of this unit
        /// </summary>
        public Team team { get { return owner.getTeam(); } }

        /// <summary>
        /// Defining a position of a player (if hes not controlling entities in the current game)
        /// Change in X means left or right movement on a minimap
        /// Change in Y means up or down movement on a minimap
        /// Change in Z means vertical movement (jumps, heightchanges on the map by climbing etc)
        /// </summary>
        public Point3D position { get; set; }

        public Facing facing { get; set; }

        public Velocity velocity { get; set; }

        public int HP { get; set; }

        public float attackrange { get; set; }

        public float supportrange { get; set; }

        public float damage { get; set; }
    }
}
