using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gamestate;
using Data.Gameevents;
using Data.Gameobjects;
using MathNet.Spatial.Functions;

namespace Preprocessing
{
    public class MainPreprocessor : IPreprocessor
    {

        static Hashtable gridsights_hash = new Hashtable();


        public void preprocessData(ReplayGamestate gamestate)
        {

            foreach (var round in gamestate.match.rounds)
                foreach (var tick in round.ticks)
                    foreach (var tevent in tick.tickevents)
                        hashGridcells(tevent);
        }

        private static void hashGridcells(Event tevent)
        {
            if (tevent.gameeventtype == "PlayerKilled" || tevent.gameeventtype == "PlayerHurt")
            {
                var kevent = tevent as PlayerHurt;
                // Hash the start and end cell
                var actor_grid_x = GridFunctions.GetGridPosX(kevent.actor.position.X, 0, 0); // TODO: read from file origin and cellwidth
                var actor_grid_y = GridFunctions.GetGridPosY(kevent.actor.position.Y, 0, 0);
                var actor_yaw = kevent.actor.facing.Yaw;

                var target_grid_x = GridFunctions.GetGridPosX(kevent.victim.position.X,0,0);
                var target_grid_y = GridFunctions.GetGridPosY(kevent.victim.position.Y,0,0);

                // Hash all cells on the way to the target with bresenham stepping TODO
                var bres_cells = performBresenham(actor_grid_x, actor_grid_y, target_grid_x, target_grid_y);
                bres_cells.ToList().ForEach(cell => gridsights_hash.Add(1,2));
            }
        }

        private static MapGridCell[] performBresenham(object actor_grid_x, object actor_grid_y, object target_grid_x, object target_grid_y)
        {
            throw new NotImplementedException();
        }
    }
}
