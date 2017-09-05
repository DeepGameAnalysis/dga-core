using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Shapes
{
    public class Node
    {
        public double XPos { get; set; }
        public double YPos { get; set; }
    }

    public class TextNode : Node
    {
        public string Text { get; set; }
    }

    public class ShapeNode : Node
    {
        public Geometry Geometry { get; set; }
        public Brush Stroke { get; set; }
        public Brush Fill { get; set; }
    }
}
