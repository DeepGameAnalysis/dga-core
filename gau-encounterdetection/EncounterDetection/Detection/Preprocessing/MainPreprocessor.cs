using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gamestate;
using Data.Gameevents;
using Data.Gameobjects;
using Data.Utils;
using MathNet.Spatial.Functions;
using Detection;

namespace Preprocessing
{
    /// <summary>
    /// Preprocessing standards for all games are computed here.
    /// Let IPreprocessors inherit this class to take over its functionality and add extra special functionality to your own Preprocessor
    /// </summary>
    public class MainPreprocessor
    {

        static Hashtable gridsights_hash = new Hashtable();


        public void PreprocessMain(ReplayGamestate gamestate, MapMetaData mapmeta, EncounterDetectionData nedData)
        {

            //foreach (var round in gamestate.match.rounds)
            //    foreach (var tick in round.ticks)
            //        foreach (var tevent in tick.tickevents)
            //            HashGridcells(tevent);
        }

        private static void HashGridcells(Event tevent)
        {
            if (tevent.GameeventType == "PlayerKilled" || tevent.GameeventType == "PlayerHurt")
            {
                var kevent = tevent as PlayerHurt;
                // Hash the start and end cell
                var actor_grid_x = GridFunctions.GetGridPosX(kevent.Actor.Position.X, 0, 0); // TODO: read from file origin and cellwidth
                var actor_grid_y = GridFunctions.GetGridPosY(kevent.Actor.Position.Y, 0, 0);
                var actor_yaw = kevent.Actor.Facing.Yaw;

                var target_grid_x = GridFunctions.GetGridPosX(kevent.Victim.Position.X,0,0);
                var target_grid_y = GridFunctions.GetGridPosY(kevent.Victim.Position.Y,0,0);

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
