using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gau_encounterdetection
{
    class Program
    {

        static void Main(string[] args)
        {
            Hashtable h = new Hashtable();

            h["names"] = "Dick";
            h[2] = "Dick2";

            Console.WriteLine(h["names"]);
            Console.WriteLine(h[2]);
        }
    }
}
