using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameobjects;
using MathNet.Spatial.Euclidean;

namespace Data.Gameevents
{
    /// <summary>
    /// Movementevents just give us a hint that we should update the current position of this player.
    /// </summary>
    public class MovementEvents : Event
    {
        /// <summary>
        /// String describing a subevent of movement event //Jumping, Sliding etc
        /// </summary>
        public string movementtype;

        public override Player[] getPlayers()
        {
            return new Player[] { actor };
        }

        public override Point3D[] getPositions()
        {
            return new Point3D[] { actor.position };
        }
    }

}
