using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameobjects;

namespace Data.Gameevents
{
    public class ServerEvents : Event
    {
        /// <summary>
        /// Type of the server event (Disconnect, Connect etc)
        /// </summary>
        public string servereventtype { get; set; }
    }

    public class TakeOverEvent : ServerEvents
    {
        /// <summary>
        /// The player which is been taken over by a bot
        /// </summary>
        public Player taken;
    }
}
