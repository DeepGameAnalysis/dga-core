using System;
using log4net;
using GAUConsole;

namespace gau_console
{
    class Program
    {

        private const string TEST_PATH = "E:/LRZ Sync+Share/Bacheloarbeit/Demofiles/downloaded valle";
        private const string DUST_ESPORT_PATH = "E:/CS GO Demofiles/dust2 esport/";
        private const string PATH = "E:/CS GO Demofiles/";


        private static ILog LOG;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            //readFilesFromCommandline(args);
            GAUConsoleApplication.readAllFiles(TEST_PATH);
            LOG.Info("All files on this path parsed. Press enter to quit.");
            Console.WriteLine("All files on this path parsed. Press enter to quit.");
            Console.ReadLine();
        }

       
    }
}
