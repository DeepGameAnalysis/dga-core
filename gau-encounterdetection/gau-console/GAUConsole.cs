using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Detection;
using JSONParsing;
using Newtonsoft.Json;
using Data.Gamestate;
using DemoInfo;
using GameStateGenerators;
using Data.Gameobjects;
using Data.Utils;
using Preprocessing;

namespace GAUConsole
{
    public class GAUConsoleApplication
    {
        private static ILog LOG;

        private static List<string> invalidfiles = new List<string>();

        /// <summary>
        /// Read all supported replay files at the given path
        /// </summary>
        /// <param name="PATH"></param>
        public static void readAllFiles(string PATH)
        {
            var filelist = Directory.EnumerateFiles(PATH, "*.dem");
            LOG.Info("Files to handle: " + filelist.Count());
            Console.WriteLine("Files to handle: " + filelist.Count());
            foreach (string file in filelist)
                readFile(file);

            foreach (string invalidfile in invalidfiles)
                Console.WriteLine("Could not parse: " + invalidfile + "\nReplay not supported yet. Please use only dust2");

        }

        /// <summary>
        /// Read all files given through the command line
        /// </summary>
        /// <param name="args"></param>
        public static void readFilesFromCommandline(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
                readFile(args[i]);
        }

        private static bool skipfile = false;


        /// <summary>
        /// Read a supported replay file
        /// </summary>
        /// <param name="demopath"></param>
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
                                CSGOGameStateGenerator.GenerateJSONFile(newdemoparser, ptask);
                                readDemoJSON(jsonpath, ptask);
                                CSGOGameStateGenerator.cleanUp();
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

        /// <summary>
        /// Read a demo json file if the corresponding replay file was already parsed 
        /// </summary>
        /// <param name="jsonpath"></param>
        /// <param name="ptask"></param>
        private static void readDemoJSON(string jsonpath, ParseTask ptask)
        {
            Console.WriteLine(jsonpath);

            using (var reader = new StreamReader(jsonpath))
            {
                var deserializedGamestate = Newtonsoft.Json.JsonConvert.DeserializeObject<ReplayGamestate>(reader.ReadToEnd(), ptask.settings);
                Console.WriteLine("Map: " + deserializedGamestate.meta.mapname);

                string metapath = @"E:\LRZ Sync+Share\Bacheloarbeit\CS GO Encounter Detection\csgo-stats-ed\CSGO Analytics\CSGO Analytics\src\views\mapviews\" + deserializedGamestate.meta.mapname + ".txt";
                //string path = @"C:\Users\Patrick\LRZ Sync+Share\Bacheloarbeit\CS GO Encounter Detection\csgo-stats-ed\CSGO Analytics\CSGO Analytics\src\views\mapviews\" + mapname + ".txt";
                var mapmeta = MapMetaDataPropertyReader.readProperties(metapath);
                LOG.Info("Detecting Encounters");
                new EncounterDetection(deserializedGamestate, new CSGOPreprocessor()).detectEncounters();
            }
        }

        /// <summary>
        /// Skip a file because it is not supported or has other errors
        /// </summary>
        /// <param name="demoparser"></param>
        /// <param name="ptask"></param>
        /// <returns></returns>
        private static bool skipFile(DemoParser demoparser, ParseTask ptask)
        {
            var mapname = CSGOGameStateGenerator.peakMapname(demoparser, ptask);
            LOG.Info("Map: " + mapname);
            if (!Map.SUPPORTED_MAPS.Contains(mapname))
                return true;

            CSGOGameStateGenerator.cleanUp();
            return false;
        }
    }
}
