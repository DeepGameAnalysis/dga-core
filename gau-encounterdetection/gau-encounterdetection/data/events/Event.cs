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
    /// All events inheriting this class should depict all necessary events for all supported games. make sure to add events which are new if you want to support a game!
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Identifier for events f.e. "player_hurt" "player_killed"
        /// </summary>
        public string gameeventtype { get; set; }

        /// <summary>
        /// The actor is always the person causing this event(or being updated with a event)
        /// </summary>
        public Player actor;

        /// <summary>
        /// Get players in this event
        /// </summary>
        /// <returns></returns>
        public virtual Player[] getPlayers() { return new Player[] { actor }; }

        /// <summary>
        /// Get positions tracked in the event
        /// </summary>
        /// <returns></returns>
        public virtual Point3D[] getPositions() { return new Point3D[] { actor.Position }; }

    }
}
