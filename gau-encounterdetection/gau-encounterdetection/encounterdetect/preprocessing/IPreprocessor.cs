using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Gamestate;
using Data.Utils;

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
        void preprocessData(ReplayGamestate gamestate, MapMetaData map);
    }
}
