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
    class WeaponFire : Event
    {
        public Weapon weapon { get; set; }

        public override Player[] getPlayers()
        {
            return new Player[] { actor };
        }

    }

    /// <summary>
    /// Event describing where a player saw another player of the opposing team
    /// </summary>
    class PlayerSpotted : Event
    {
        public Player spotter { get; set; } //TODO: how find out spotter? or do this in algorithm?

        public override Player[] getPlayers()
        {
            return new Player[] { actor };
        }
    }

    /// <summary>
    /// Event describing dealt damage from a attacker to a vicitim
    /// </summary>
    class PlayerHurt : Event
    {
        public Player victim { get; set; }

        public int HP { get; set; }

        public int armor { get; set; }

        public int armor_damage { get; set; }

        public int HP_damage { get; set; }

        public int hitgroup { get; set; }

        public Weapon weapon { get; set; }

        public override Player[] getPlayers()
        {
            return new Player[] { actor, victim };
        }

        public override Point3D[] getPositions()
        {
            return new Point3D[] { actor.position, victim.position };
        }
    }

    /// <summary>
    /// Event describing a death of a player caused by another player
    /// </summary>
    class PlayerKilled : PlayerHurt
    {

        public bool headshot { get; set; }

        public int penetrated { get; set; }

        public Player assister { get; set; }

        public override Player[] getPlayers()
        {
            if (assister != null)
                return new Player[] { actor, victim, assister };
            else
                return base.getPlayers();
        }

        public override Point3D[] getPositions()
        {
            if(assister != null)
                return new Point3D[] { actor.position, assister.position, victim.position };
            else
                return new Point3D[] { actor.position, victim.position };
        }
    }
}
