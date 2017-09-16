using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Gameobjects
{
    public class Unit : Entity
    {
        /// <summary>
        /// Player who controlls this unit
        /// </summary>
        public Player Owner { get; set; }

        /// <summary>
        /// Team of the player of this unit
        /// </summary>
        public Team Team { get { return Owner.Team; } }
    }
}
