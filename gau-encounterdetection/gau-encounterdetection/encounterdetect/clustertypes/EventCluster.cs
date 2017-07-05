using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gameevents;

namespace Clustering
{
    /// <summary>
    /// High dimensional cluster of event data (player, eventtype, damage, position, vicitim/beneficiary of the event etc)
    /// </summary>
    public class EventCluster : Cluster<Event>
    {
    }
}
