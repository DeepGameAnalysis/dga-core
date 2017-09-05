using Data.Gameobjects;
using System.Collections.Generic;
using System.Linq;

namespace Data.Gamestate
{
    /// <summary>
    /// !! DO NOT CONFUSE THIS WITH THE TERM "GAMESTATE" IN VIDEO GAME DEVELOPMENT FOR THE CURRENT STATE OF THE GAME. !!
    /// !! THIS IS EVERY GAMESTATE CHANGE IN ONE BIG REPLAYGAMESTATE OBJECT !!
    /// This holds our entire game from tick 1 till end. 
    /// </summary>
    public class ReplayGamestate
    {
        /// <summary>
        /// Meta data about this match`s gamestate
        /// </summary>
        public ReplayGamstateMeta Meta { get; set; }

        /// <summary>
        /// The data about the match itself
        /// </summary>
        public Match Match { get; set; }
    }

    public class ReplayGamstateMeta
    {
        public int gamestate_id { get; set; }

        public string Mapname { get; set; }

        public float Tickrate { get; set; }

        public int Tickcount { get; set; }

        public List<PlayerMeta> Players { get; set; }
    }

    public class Match
    {
        public Team WinnerTeam { get; set; }

        public List<Player> Winners { get; set; }

        public List<Round> Rounds { get; set; }

        /// <summary>
        /// Get tickrange of this match.
        /// </summary>
        /// <returns></returns>
        public int GetRoundTickRange()
        {
            int range = 0;
            foreach (var round in Rounds)
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

        public string winner_team { get; set; }

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
