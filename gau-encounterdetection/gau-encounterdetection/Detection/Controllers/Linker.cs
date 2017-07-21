using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gamestate;

namespace Detection
{
    public enum CombatlinkSettings
    {
        /// <summary>
        /// Search combatlinks which are found by line-of-sight
        /// </summary>
        SIGHT_BASED,
        /// <summary>
        /// Search combatlinks based on events only -> each event can create a link in his tick
        /// </summary>
        EVENT_BASED,
        /// <summary>
        /// Search combatlinks based on distance.(if distance less or equal than attackrange then build combatlink)
        /// </summary>
        DISTANCE_BASED
    }

    /// <summary>
    /// Initalize the linker depending on which method you want to use to find links in your game. The linker is searching for links between entities
    /// </summary>
    public class Linker
    {
        private CombatlinkSettings csetting;

        public Linker(CombatlinkSettings csetting)
        {
            this.csetting = csetting;
        }

        public void findLinks(Tick t)
        {
            switch (csetting)
            {
                case CombatlinkSettings.SIGHT_BASED:
                    break;
                case CombatlinkSettings.EVENT_BASED:
                    break;
                case CombatlinkSettings.DISTANCE_BASED:
                    break;
            }
        }


    }
}
