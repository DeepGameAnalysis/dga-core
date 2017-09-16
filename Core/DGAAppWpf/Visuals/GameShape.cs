using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Shapes
{
    public class GameShape 
    {
        public bool Active { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public Brush Stroke { get; set; }

        public Brush Fill { get; set; }

        public double StrokeThickness { get; internal set; }
    }
}
