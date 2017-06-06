using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameobjects;

namespace Data.Gamestate
{
    /// <summary>
    /// !! DO NOT CONFUSE THIS WITH THE TERM "GAMESTATE" IN VIDEO GAME DEVELOPMENT FOR THE CURRENT STATE OF THE GAME. !!
    /// !! THIS IS EVERY GAMESTATE CHANGE IN ON BIG REPLAYGAMESTATE OBJECT !!
    /// This holds our entire game from tick 1 till end. 
    /// </summary>
    public class ReplayGamestate
    {
        /// <summary>
        /// Meta data about this match`s gamestate
        /// </summary>
        public ReplayGamstateMeta meta { get; set; }

        /// <summary>
        /// The data about the match itself
        /// </summary>
        public Match match { get; set; }
    }

    public class ReplayGamstateMeta
    {
        public int gamestate_id { get; set; }

        public string mapname { get; set; }

        public float tickrate { get; set; }

        public int tickcount { get; set; }

        public List<PlayerDetailed> players { get; set; }
    }

    public class Match
    {
        public Team winnerteam { get; set; }

        public List<Player> winners { get; set; }

        public List<Round> rounds { get; set; }

        /// <summary>
        /// Get tickrange of this match.
        /// </summary>
        /// <returns></returns>
        public int getRoundTickRange()
        {
            int range = 0;
            foreach (var round in rounds)
                range += round.getRoundTickRange();
            return range;
        }
    }

    /// <summary>
    /// Every game does at least have one round
    /// </summary>
    public class Round
    {
        public int round_id { get; set; }

        public Player winner_player { get; set; }

        public Team winner_team { get; set; }

        public List<Tick> ticks { get; set; }

        public int max_tick_id { get; set; }

        public int min_tick_id { get; set; }

        /// <summary>
        /// Get range of ticks in a round.
        /// </summary>
        /// <returns></returns>
        public int getRoundTickRange()
        {
            if(min_tick_id == 0)
                min_tick_id = ticks.Min(tick => tick.tick_id);
            if (max_tick_id == 0)
                max_tick_id = ticks.Max(tick => tick.tick_id);

            return (max_tick_id - min_tick_id);
        }
    }
}
