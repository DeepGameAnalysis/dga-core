using MathNet.Spatial.Euclidean;
using System.Collections.Generic;

namespace Data.Gameobjects
{
    public enum Team { //@TODO: Split for each game
        //CSGO
        None,
        CT,
        T,
        //Age of Empires
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight
    }

    public class PlayerMeta
    {
        public string playername { get; set; }

        public long player_id { get; set; }

        public string team { get; set; }

        public string clanname { get; set; }
    }

    public class Player
    {
        public const int CSGO_PLAYERMODELL_HEIGHT = 72;
        public const int CSGO_PLAYERMODELL_CROUCH_HEIGHT = 54;
        public const int CSGO_PLAYERMODELL_WIDTH = 32;
        public const int CSGO_PLAYERMODELL_JUMPHEIGHT = 54;

        public string Playername { get; set; }

        public long player_id { get; set; }

        public string Team { get; set; }

        /// <summary>
        /// Defining a position of a player (if hes not controlling entities in the current game)
        /// Change in X means left or right movement on a minimap
        /// Change in Y means up or down movement on a minimap
        /// Change in Z means vertical movement (jumps, heightchanges on the map by climbing etc)
        /// </summary>
        public Point3D Position { get; set; }

        public Facing Facing { get; set; }

        /// <summary>
        /// Velocity changes correlate with the positioning as mentioned for a players "position"
        /// </summary>
        public Velocity Velocity { get; set; }

        public int HP { get; set; }

        public bool IsSpotted { get; set; }

        /// <summary>
        /// Maps strings back to Team.Enum
        /// </summary>
        /// <returns></returns>
        public Team GetTeam()
        {
            if(Team == "Terrorist")
            {
                return Gameobjects.Team.T;
            } else if(Team == "CounterTerrorist")
            {
                return Gameobjects.Team.CT;
            }
            return Gameobjects.Team.None;

        }

        public bool SameTeam(Player p)
        {
            if (GetTeam() == p.GetTeam())
                return true;

            return false;
        }

        public bool IsDead()
        {
            if (HP == 0)
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return "Name: " + Playername + " ID: "+ player_id + " Team: " +Team;
        }

        public override bool Equals(object obj) //Why does a true overriden Equals kill the json serialisation?!?
        {
            Player p = obj as Player;
            if (p == null)
                return false;
            if (player_id == p.player_id)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.player_id.GetHashCode();
        }

    }

    public class PlayerDetailed : Player
    {
        public int Armor { get; set; }
        public bool HasHelmet { get; set; }
        public bool HasDefuser { get; set; }
        public bool HasBomb { get; set; }
        public bool IsDucking { get; set; }
        public bool IsWalking { get; set; }
        public bool IsScoped { get; set; }

        public List<Item> Items { get; set; }

        public Item GetPrimaryWeapon()
        {
            if (Items[0] == null)
                //errorlog
                return null;

            return Items[0];
        }
    }

    public class PlayerFlashed : Player
    {
        public float Flashedduration { get; set; }
    }

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
