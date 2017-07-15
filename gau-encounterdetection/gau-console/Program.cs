using System;
using log4net;
using GAUConsole;

namespace gau_console
{
    class Program
    {

        private const string TEST_PATH = "D:/Ressources/Test CSGO";

        private static ILog LOG;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            //readFilesFromCommandline(args);
            GAUConsoleApplication.ReadAllFiles(TEST_PATH);
            LOG.Info("All files on this path parsed. Press enter to quit.");
            Console.WriteLine("All files on this path parsed. Press enter to quit.");
            Console.ReadLine();
        }

       
    }
}
