using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameobjects;
using Data.Gamestate;
using Data.Gameevents;

namespace Detection
{
    /// <summary>
    /// Class to save all relevant data to replay the entire match with its encounters and events.
    /// Tick is holding all the events to draw while component is holding all links to draw between players
    /// </summary>
    public class EncounterDetectionReplay : IDisposable
    {
        /// <summary>
        ///  All tick and component pairs saved in a dicitionary.
        /// </summary>
        Dictionary<Tick, CombatComponent> data = new Dictionary<Tick, CombatComponent>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="comp"></param>
        public void insertReplaydata(Tick tick, CombatComponent comp) //@TODO: save encounters instead of components
        {
            if (tick.tick_id != comp.tick_id) throw new Exception("Cannot save replaydata. Component and Tickdata are not matching");
            data.Add(tick, comp);
        }

        /// <summary>
        /// Returns the next tick and removes the pair -> at the end the dictionary is cleared
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<Tick, CombatComponent> getNextTick()
        {
            var first = data.First();
            data.Remove(first.Key);
            return first;
        }

        /// <summary>
        /// Return all ticks with same or higher tick_id(inclusive)
        /// </summary>
        /// <param name="tick_id"></param>
        /// <returns></returns>
        public List<KeyValuePair<Tick,CombatComponent>> getTicksFrom(int tick_id)
        {
            return data.Where(t => t.Key.tick_id >= tick_id).ToList();
        }

        /// <summary>
        /// Return all ticks with same or lower tick_id(inclusive)
        /// </summary>
        /// <param name="tick_id"></param>
        /// <returns></returns>
        public List<KeyValuePair<Tick,CombatComponent>> getTicksUntil(int tick_id)
        {
            return data.Where(t => t.Key.tick_id <= tick_id).ToList();
        }

        /// <summary>
        /// Check if this replays data has invalid tick/component pairs. f.e. tick t has player A with different data in its component(health not the same etc)
        /// </summary>
        private void checkIntegrity()
        {

        }

        public Dictionary<Tick, CombatComponent> getReplayData()
        {
            return data;
        }

        public void Dispose()
        {
            data = null;
        }
    }
}
