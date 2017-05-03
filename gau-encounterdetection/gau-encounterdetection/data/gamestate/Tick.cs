using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameobjects;
using Data.Gameevents;

namespace Data.Gamestate
{
    public class Tick
    {
        public int tick_id { get; set; }

        public List<Event> tickevents { get; set; }

        /// <summary>
        /// Return all players mentioned in a given tick.
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        public List<Player> getUpdatedPlayers() //TODO: what happens if one player is added multiple times
        {
            List<Player> ps = new List<Player>();
            foreach (var g in tickevents)
                ps.AddRange(g.getPlayers()); //Every gameevent provides its acting players
            
            return ps;
        }

        public List<NadeEvents> getNadeEvents()
        {
            return tickevents.Where(t => t.gameeventtype == "smoke_exploded"  || t.gameeventtype == "fire_exploded" || t.gameeventtype == "hegrenade_exploded" || t.gameeventtype == "flash_exploded").Cast<NadeEvents>().ToList();
        }

        public List<NadeEvents> getNadeEndEvents()
        {
            return tickevents.Where(t => t.gameeventtype == "smoke_ended" || t.gameeventtype == "firenade_ended" || t.gameeventtype == "hegrenade_exploded" || t.gameeventtype == "flash_exploded").Cast<NadeEvents>().ToList();
        }

        public List<ServerEvents> getServerEvents()
        {
            return tickevents.Where(t => t.gameeventtype == "player_bind" || t.gameeventtype == "player_disconnected").Cast<ServerEvents>().ToList();
        }

        public List<Event> getTickevents()
        {
            return tickevents.Where(e => e.gameeventtype != "player_bind" || e.gameeventtype != "player_disconnected").ToList();
        }
    }


}
