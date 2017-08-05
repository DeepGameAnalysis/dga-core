using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Data.Utils
{
    /// <summary>
    /// Every meta data about a map is located here
    /// </summary>
    public class MapMetaData
    {
        public string Mapname { get; set; }

        public double CoordOriginX { get; set; }

        public double CoordOriginY { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double scale;

        public int rotate { get; set; }

        public double zoom { get; set; }

        public override string ToString()
        {
            return "Mapname: " + Mapname + " Center: " + CoordOriginX +","+CoordOriginY+ " Dimension: " +Width +","+Height ;
        }

    }


    public class MapMetaDataPropertyReader
    {
        /// <summary>
        /// Reads a map info file "<mapname>".txt and extracts the relevant data about the map
        /// </summary>
        /// <param name="path"></param>
        public static MapMetaData ReadProperties(string path)
        {
            string line;

            var fmt = new NumberFormatInfo();
            fmt.NegativeSign = "-";

            MapMetaData metadata = new MapMetaData();
            using (var file = new StreamReader(path))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var resultString = Regex.Match(line, @"-?\d+").Value; //Match negative and positive int numbers

                    if (line.Contains("pos_x"))
                    {
                        metadata.CoordOriginX = double.Parse(resultString, fmt);
                    }
                    else if (line.Contains("pos_y"))
                    {
                        metadata.CoordOriginY = double.Parse(resultString, fmt);
                    }
                    else if (line.Contains("scale"))
                    {
                        metadata.scale = Double.Parse(resultString);
                    }
                    else if (line.Contains("rotate"))
                    {
                        metadata.rotate = Int32.Parse(resultString);
                    }
                    else if (line.Contains("zoom"))
                    {
                        metadata.zoom = Double.Parse(resultString);
                    }
                    else if (line.Contains("width"))
                    {
                        metadata.Width = double.Parse(resultString, fmt);
                    }
                    else if (line.Contains("height"))
                    {
                        metadata.Height = double.Parse(resultString, fmt);
                    }
                }

                file.Close();
            }
            return metadata;
        }
    }
}
