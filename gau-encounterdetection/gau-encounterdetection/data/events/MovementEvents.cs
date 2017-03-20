using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameobjects;

namespace Data.Gameevents
{
    /// <summary>
    /// Movementevents just give us a hint that we should update the current position of this player.
    /// </summary>
    class MovementEvents : Event
    {

        public override Player[] getPlayers()
        {
            return new Player[] { actor };
        }

        public override EDVector3D[] getPositions()
        {
            return new EDVector3D[] { actor.position };
        }
    }
}
