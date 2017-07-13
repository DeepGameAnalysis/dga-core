using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Data.Gameobjects;
using MathNet.Spatial.Euclidean;

namespace Data.Gameevents
{
    /// <summary>
    /// Event describing a location where a player fired a shot at.
    /// </summary>
    public class WeaponFire : Event
    {
        public Item Weapon { get; set; }

        public override Player[] getPlayers()
        {
            return new Player[] { actor };
        }

    }

    /// <summary>
    /// Event describing where a player saw another player of the opposing team
    /// </summary>
    public class PlayerSpotted : Event
    {
        public Player Spotter { get; set; } //TODO: how find out spotter? or do this in algorithm?

        public override Player[] getPlayers()
        {
            return new Player[] { actor };
        }
    }

    /// <summary>
    /// Event describing dealt damage from a attacker to a vicitim
    /// </summary>
    public class PlayerHurt : WeaponFire
    {
        public Player Victim { get; set; }

        public int HP { get; set; }

        public int Armor { get; set; }

        public int Armor_damage { get; set; }

        public int HP_damage { get; set; }

        public int Hitgroup { get; set; }

        public override Player[] getPlayers()
        {
            return new Player[] { actor, Victim };
        }

        public override Point3D[] getPositions()
        {
            return new Point3D[] { actor.Position, Victim.Position };
        }
    }

    /// <summary>
    /// Event describing a death of a player caused by another player
    /// </summary>
    public class PlayerKilled : PlayerHurt
    {

        public bool Headshot { get; set; }

        public int Penetrated { get; set; }

        public Player Assister { get; set; }

        public override Player[] getPlayers()
        {
            if (Assister != null)
                return new Player[] { actor, Victim, Assister };
            else
                return base.getPlayers();
        }

        public override Point3D[] getPositions()
        {
            if(Assister != null)
                return new Point3D[] { actor.Position, Assister.Position, Victim.Position };
            else
                return new Point3D[] { actor.Position, Victim.Position };
        }
    }
}
