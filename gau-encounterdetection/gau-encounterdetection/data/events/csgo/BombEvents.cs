using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameobjects;

namespace Data.Gameevents
{
    /// <summary>
    /// Events for bomb planted, defused, abort plant and abort defuse for all games supporting a carry and plant bomb mode.
    /// </summary>
    class BombEvents : Event
    {
        public char site { get; set; }

        public bool haskit { get; set; }

        public override Player[] getPlayers()
        {
            return new Player[] { actor };
        }
    }

    /// <summary>
    /// Events for bomb pickup and bomb drop
    /// </summary>
    public class BombState : Event
    {

        public override Player[] getPlayers()
        {
            return new Player[] { actor };
        }
    }
}
