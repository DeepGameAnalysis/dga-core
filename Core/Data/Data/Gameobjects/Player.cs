using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;

namespace Data.Gameobjects
{
    public enum Team
    { //@TODO: Split for each game
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

    public class PlayerMeta : Player
    {
        public string Clanname { get; set; }
    }

    public class Player : Entity
    {
        public const int CSGO_PLAYERMODELL_HEIGHT = 72;
        public const int CSGO_PLAYERMODELL_CROUCH_HEIGHT = 54;
        public const int CSGO_PLAYERMODELL_WIDTH = 32;
        public const int CSGO_PLAYERMODELL_JUMPHEIGHT = 54;

        public string Playername { get; set; }

        public long player_id { get; set; }

        /// <summary>
        /// Controlled entities by this player (nades, units, other AI or objects of this player which are networked)
        /// </summary>
        public HashSet<Unit> Units;

        /// <summary>
        /// Team of the player of this unit
        /// </summary>
        public Team Team { get; set; }


        public bool SameTeam(Player p)
        {
            if (Team == p.Team)
                return true;

            return false;
        }

        public bool IsDead()
        {
            if (HP == 0)
                return true;
            return false;
        }

        public override string ToString()
        {
            return "Name: " + Playername + " ID: " + player_id + " Team: " + Team;
        }

        public override bool Equals(object obj) //Why does a true overriden Equals kill the json serialisation?!?
        {
            Player p = obj as Player;
            if (p == null)
                return false;

            if (player_id == p.player_id)
            {
                if (!SameTeam(p))
                    throw new Exception("Player with same IDs cannot be in different teams!");
                if (Playername.Trim().Equals(p.Playername.Trim()))
                    throw new Exception("Player with same IDs cannot be in different teams!");

                return true;
            }

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


}
