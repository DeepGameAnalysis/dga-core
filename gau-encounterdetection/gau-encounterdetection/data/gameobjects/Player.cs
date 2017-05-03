using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public const int PLAYERMODELL_HEIGHT = 72;
        public const int PLAYERMODELL_CROUCH_HEIGHT = 54;
        public const int PLAYERMODELL_WIDTH = 32;
        public const int PLAYERMODELL_JUMPHEIGHT = 54;

        public string playername { get; set; }

        public long player_id { get; set; }

        public string team { get; set; }

        public Point3D position { get; set; }

        public Facing facing { get; set; }

        public Velocity velocity { get; set; }

        public int HP { get; set; }

        public bool isSpotted { get; set; }

        /// <summary>
        /// Maps strings back to Team.Enum
        /// </summary>
        /// <returns></returns>
        public Team getTeam()
        {
            if(team == "Terrorist")
            {
                return Team.T;
            } else if(team == "CounterTerrorist")
            {
                return Team.CT;
            }
            return Team.None;

        }

        public bool sameTeam(Player p)
        {
            if (getTeam() == p.getTeam())
                return true;

            return false;
        }

        public bool isDead()
        {
            if (HP == 0)
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return "Name: " + playername + " ID: "+ player_id + " Team: " +team;
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

        public Player Copy()
        {
            Player me = new Player();
            me.player_id = player_id;

            me.playername = playername;
            me.team = team;
            me.position = position.Copy();
            me.facing = facing.Copy();


            return me;
        }
    }

    public class PlayerDetailed : Player
    {
        public int armor { get; set; }
        public bool hasHelmet { get; set; }
        public bool hasDefuser { get; set; }
        public bool hasBomb { get; set; }
        public bool isDucking { get; set; }
        public bool isWalking { get; set; }
        public bool isScoped { get; set; }
    }

    public class PlayerDetailedWithItems : PlayerDetailed
    {
        public List<CSGOWeapon> items { get; set; }

        public CSGOWeapon getPrimaryWeapon()
        {
            if (items[0] == null)
                //errorlog
                return null;

            return items[0];
        }
    }

    public class PlayerFlashed : Player
    {
        public float flashedduration { get; set; }
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

        internal float[] getAsArray()
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

        internal float[] getAsArray()
        {
            return new float[] { VX, VY, VZ };
        }
    }
}
