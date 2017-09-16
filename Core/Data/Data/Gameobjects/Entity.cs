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

        public bool IsSpotted { get; set; }
    }


    //
    //
    // ELEMENTARY CLASSES FOR ENTITIES HERE
    //
    //



    /// <summary>
    /// Facing-class: Holding the direction of sight if given. For example as yaw and pitch values
    /// </summary>
    public class Facing
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }

        internal Facing Copy()
        {
            return new Facing() { Yaw = Yaw, Pitch = Pitch };
        }

        internal float[] GetAsArray()
        {
            return new float[] { Yaw, Pitch };
        }
    }

    /// <summary>
    /// Veolcity-class: Holding every component of a velocity vector
    /// </summary>
    public class Velocity
    {
        public float VX { get; set; }
        public float VY { get; set; }
        public float VZ { get; set; }

        internal Velocity Copy()
        {
            return new Velocity() { VX = VX, VY = VY, VZ = VZ };
        }

        internal float[] GetAsArray()
        {
            return new float[] { VX, VY, VZ };
        }
    }
}
