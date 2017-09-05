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
        public List<Player> GetUpdatedPlayers() //TODO: what happens if one player is added multiple times
        {
            List<Player> ps = new List<Player>();
            foreach (var g in tickevents)
                ps.AddRange(g.getPlayers()); //Every gameevent provides its acting players
            
            return ps;
        }

        public List<NadeEvents> getNadeEvents()
        {
            return tickevents.Where(t => t.GameeventType == "smoke_exploded"  || t.GameeventType == "fire_exploded" || t.GameeventType == "hegrenade_exploded" || t.GameeventType == "flash_exploded").Cast<NadeEvents>().ToList();
        }

        public List<NadeEvents> getNadeEndEvents()
        {
            return tickevents.Where(t => t.GameeventType == "smoke_ended" || t.GameeventType == "firenade_ended" || t.GameeventType == "hegrenade_exploded" || t.GameeventType == "flash_exploded").Cast<NadeEvents>().ToList();
        }

        public List<ServerEvents> getServerEvents()
        {
            return tickevents.Where(t => t.GameeventType == "player_bind" || t.GameeventType == "player_disconnected").Cast<ServerEvents>().ToList();
        }

        public List<Event> getTickevents()
        {
            return tickevents.Where(e => e.GameeventType != "player_bind" || e.GameeventType != "player_disconnected").ToList();
        }
    }


}
