using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameobjects;
using MathNet.Spatial.Euclidean;

namespace Detection
{
    public enum LinkType { COMBATLINK, SUPPORTLINK };

    public enum Direction { UNDIRECTED, DEFAULT }; // DEFAULT means Link is directed from actor to reciever, UNDIRECTED means to each other

    public class Link
    {
        /// <summary>
        /// Type of the Link
        /// </summary>
        private LinkType type;

        /// <summary>
        /// Players contained in this Link. Use getActor() and getReciever() to get single participants.
        /// </summary>
        private Player[] players;

        /// <summary>
        /// Direction of this Link
        /// </summary>
        private Direction dir;

        /// <summary>
        /// Impact done by this link. Can either be the amount of damage or heal or buff etc aka it is the weight to this edge
        /// </summary>
        public double Impact { get; set; }

        /// <summary>
        /// Defines if this link was built with a kill event
        /// </summary>
        public bool IsKill { get; set; }

        /// <summary>
        /// Position of a collision with an obstacle by this link
        /// </summary>
        public Point2D? coll;

        public Link()
        {

        }

        public Link(Player actor, Player reciever, LinkType type, Direction dir)
        {
            if (actor == null || reciever == null) throw new Exception("Players cannot be null");
            if (actor.GetTeam() != reciever.GetTeam() && type == LinkType.SUPPORTLINK)
            {
                Console.WriteLine("Cannot create Supportlink between different teams"); // Occurs if a kill occurs where an enemy hit his teammate so hard that he is registered as assister
            }
            if (actor.GetTeam() == reciever.GetTeam() && type == LinkType.COMBATLINK)
            {
                Console.WriteLine("Cannot create Combatlink in the same team"); //Can occur if teamdamage happens. Dman antimates
            }

            players = new Player[2];
            players[0] = actor;
            players[1] = reciever;
            this.type = type;
            this.dir = dir;
        }

        public Link(Player actor, Player reciever, LinkType type, Direction dir, Point2D? coll)
        {
            if (actor == null || reciever == null) throw new Exception("Players cannot be null");
            if (actor.GetTeam() != reciever.GetTeam() && type == LinkType.SUPPORTLINK)
                Console.WriteLine("Cannot create Supportlink between different teams"); // Occurs if a kill occurs where an enemy hit his teammate so hard that he is registered as assister
            if (actor.GetTeam() == reciever.GetTeam() && type == LinkType.COMBATLINK)
                Console.WriteLine("Cannot create Combatlink in the same team"); //Can occur if teamdamage happens. Dman antimates

            players = new Player[2];
            players[0] = actor;
            players[1] = reciever;
            this.type = type;
            this.dir = dir;
            this.coll = coll;
        }

        public bool IsUndirected()
        {
            return dir == Direction.UNDIRECTED;
        }

        public Player GetActor()
        {
            return players[0];
        }

        public Player GetReciever()
        {
            return players[1];
        }

        public LinkType GetLinkType()
        {
            return type;
        }

        public double GetImpact()
        {
            return Impact;
        }

        override public string ToString()
        {
            return type.ToString() + " | Actor: " + players[0].Playername + "- Reciever: " + players[1].Playername;
        }

        override public bool Equals(object other)
        {
            var link = other as Link;
            if (link == null)
                return false;
            if (GetActor().Equals(link.GetActor()) && GetReciever().Equals(link.GetReciever()) && dir == link.dir)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
