using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Detection;
using log4net;
using System.IO;
using JSONParser;
using Newtonsoft.Json;
using Data.Gamestate;

namespace gau_console
{
    class Program
    {

        private const string TEST_PATH = "E:/LRZ Sync+Share/Bacheloarbeit/Demofiles/downloaded valle";
        private const string DUST_ESPORT_PATH = "E:/CS GO Demofiles/dust2 esport/";
        private const string PATH = "E:/CS GO Demofiles/";

        private static EncounterDetection ed_algorithm;

        private static List<string> invalidfiles = new List<string>();

        private static ILog LOG;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            //readFilesFromCommandline(args);
            readAllFiles();
            LOG.Info("All files on this path parsed. Press enter to quit.");
            Console.WriteLine("All files on this path parsed. Press enter to quit.");
            Console.ReadLine();
        }

        private static void readAllFiles()
        {
            var filelist = Directory.EnumerateFiles(PATH, "*.dem");
            LOG.Info("Files to handle: " + filelist.Count());
            Console.WriteLine("Files to handle: " + filelist.Count());
            foreach (string file in filelist)
            {
                readFile(file);
            }

            foreach (string invalidfile in invalidfiles)
            {
                Console.WriteLine("Could not parse: " + invalidfile);
                Console.WriteLine("Replay not supported yet. Please use only dust2");
            }
        }


        private static void readFilesFromCommandline(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                readFile(args[i]);
            }
        }

        private static bool skipfile = false;


        private static void readFile(string demopath)
        {
            ParseTask ptask = new ParseTask
            {
                destpath = demopath,
                srcpath = demopath,
                usepretty = true,
                showsteps = true,
                specialevents = true,
                highdetailplayer = true,
                positioninterval = 250,
                settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.None }
            };

            LOG.Info("Reading: " + Path.GetFileName(demopath));
            try
            {
                var jsonpath = demopath.Replace(".dem", ".json");
                if (File.Exists(jsonpath))
                {
                    if (new FileInfo(jsonpath).Length == 0)
                    {
                        LOG.Info("File empty");
                        return; //File was empty -> skipped parsing it but wrote json
                    }
                    LOG.Info(".dem file already parsed");
                    readDemoJSON(jsonpath, ptask);
                }
                else
                {
                    using (var demoparser = new DemoParser(File.OpenRead(demopath)))
                    {
                        skipfile = skipFile(demoparser, ptask);
                        if (!skipfile)
                        {
                            LOG.Info("Parsing .dem file");
                            using (var newdemoparser = new DemoParser(File.OpenRead(demopath)))
                            {
                                GameStateGenerator.GenerateJSONFile(newdemoparser, ptask);
                                readDemoJSON(jsonpath, ptask);
                                GameStateGenerator.cleanUp();
                            }
                        }
                        else
                        {
                            LOG.Info("----- Not supported. Skip file: " + demopath + "-----");
                            return;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                LOG.Error(e.Message);
                LOG.Error(e.StackTrace);
                LOG.Info("----- Error occured. Skip file: " + demopath + "-----");
                return;
            }

            LOG.Info("----- Parsing and Encounter Detection was sucessful ----- ");

        }

        private static void readDemoJSON(string jsonpath, ParseTask ptask)
        {
            Console.WriteLine(jsonpath);

            using (var reader = new StreamReader(jsonpath))
            {
                var deserializedGamestate = Newtonsoft.Json.JsonConvert.DeserializeObject<ReplayGamestate>(reader.ReadToEnd(), ptask.settings);
                //reader.Close();
                Console.WriteLine("Map: " + deserializedGamestate.meta.mapname);

                string metapath = @"E:\LRZ Sync+Share\Bacheloarbeit\CS GO Encounter Detection\csgo-stats-ed\CSGO Analytics\CSGO Analytics\src\views\mapviews\" + deserializedGamestate.meta.mapname + ".txt";
                //string path = @"C:\Users\Patrick\LRZ Sync+Share\Bacheloarbeit\CS GO Encounter Detection\csgo-stats-ed\CSGO Analytics\CSGO Analytics\src\views\mapviews\" + mapname + ".txt";
                var mapmeta = MapMetaDataPropertyReader.readProperties(metapath);
                LOG.Info("Detecting Encounters");
                ed_algorithm = new EncounterDetection(deserializedGamestate, mapmeta);
                ed_algorithm.detectEncounters();
            }
        }

        private static bool skipFile(DemoParser demoparser, ParseTask ptask)
        {
            var mapname = GameStateGenerator.peakMapname(demoparser, ptask);
            LOG.Info("Map: " + mapname);
            if (!Map.SUPPORTED_MAPS.Contains(mapname))
                return true;

            GameStateGenerator.cleanUp();
            return false;
        }
    }
}
