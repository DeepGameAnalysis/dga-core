using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gamestate;
using Data.Utils;
using Detection;

namespace Preprocessing
{
    /// <summary>
    /// The preprocessor collects all necessary data from the gamestate or a various files and provides this data for its
    /// corresponding encounter detection.
    /// </summary>
    public interface IPreprocessor
    {
        /// <summary>
        /// 
        /// </summary>
        void PreprocessData(ReplayGamestate gamestate, MapMetaData map, out EncounterDetectionData edData);
    }
}
