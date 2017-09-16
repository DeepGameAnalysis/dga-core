using System;
using log4net;
using EDApplication;

namespace gau_console
{
    class Program
    {

        private const string TEST_PATH = "D:/Ressources/Test CSGO";

        static void Main(string[] args)
        {
            //readFilesFromCommandline(args);
            DemoDataIO.ReadAllFiles(TEST_PATH);
            Console.WriteLine("All files on this path parsed. Press enter to quit.");
            Console.ReadLine();
        }

       
    }
}
