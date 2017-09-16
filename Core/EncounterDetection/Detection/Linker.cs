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
        /// Search combatlinks which are found by line-of-sight tests with a map model
        /// </summary>
        SIGHT_BASED_MAPMODELL,
        /// <summary>
        /// Search links based on direct event data only
        /// </summary>
        DIRECT_EVENT_BASED,
        /// <summary>
        /// Search links based on indirect event(event relations)
        /// </summary>
        INDIRECT_EVENT_BASED,
        /// <summary>
        /// Search links based on average distances for support and attackevents.
        /// </summary>
        DISTANCE_BASED_AVG,
        /// <summary>
        /// Search links based on cluster distances for support and attackevents.
        /// </summary>
        DISTANCE_BASED_CLUSTERED,
        /// <summary>
        /// Use the default linkfinding - customized for best performance and results
        /// </summary>
        DEFAULT
    }

    /// <summary>
    /// Initalize the linker depending on which method you want to use to find links in your game. The linker is searching for links between entities
    /// </summary>
    public class Linker
    {
        private CombatlinkSettings Csetting;
        
        public Linker(CombatlinkSettings csetting)
        {
            this.Csetting = csetting;
        }

        public void FindLinks(Tick t)
        {
            switch (Csetting)
            {
                case CombatlinkSettings.SIGHT_BASED_MAPMODELL:
                    break;
                case CombatlinkSettings.INDIRECT_EVENT_BASED:
                    break;
                case CombatlinkSettings.DIRECT_EVENT_BASED:
                    break;
                case CombatlinkSettings.DISTANCE_BASED_AVG:
                    break;
                case CombatlinkSettings.DISTANCE_BASED_CLUSTERED:
                    break;
                case CombatlinkSettings.DEFAULT:
                    break;
            }
        }

        public void SetCSetting(CombatlinkSettings csetting)
        {
            this.Csetting = csetting;
        }

        public CombatlinkSettings GetCSetting()
        {
            return Csetting;
        }
    }
}
