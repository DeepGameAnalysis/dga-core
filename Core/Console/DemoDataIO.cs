using Data.Gameobjects;
using Data.Gamestate;
using Data.Utils;
using DemoInfo;
using Detection;
using GameStateGenerators;
using JSONParsing;
using Newtonsoft.Json;
using Preprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EDApplication
{
    public class DemoDataIO
    {
        private const string META_PATH = @"D:\Ressources\CS GO Demofiles\CS GO Mapmetadata\";

        private static readonly ParseTaskSettings DEFAULT_TASK = new ParseTaskSettings()
        {
            SrcPath = META_PATH,
            DestPath = META_PATH,
            HighDetailPlayer = false,
            PositionUpdateInterval = 250,
            usepretty = true,
            ShowSteps = true,
            Specialevents = true,
            Settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.None }
        };

        private static List<string> InvalidFiles = new List<string>();

        private static GameStateGenerator Generator;



        /// <summary>
        /// Read all supported replay files at the given path
        /// </summary>
        /// <param name="PATH"></param>
        public static void ReadAllFiles(string PATH)
        {
            var filelist = Directory.EnumerateFiles(PATH, "*.dem");
            Console.WriteLine("Files to handle: " + filelist.Count());
            foreach (string file in filelist)
                ParseDemoAndReadFile(file);

            foreach (string invalidfile in InvalidFiles)
                Console.WriteLine("Could not parse: " + invalidfile + "\nReplay not supported yet. Please use only dust2");

        }

        /// <summary>
        /// Read all files given through the command line
        /// </summary>
        /// <param name="args"></param>
        public static void ReadFilesFromCommandline(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
                ParseDemoAndReadFile(args[i]);
        }

        /// <summary>
        /// Decide weather a file needs skipping because it is not supported or just attentionaly needs skipping
        /// </summary>
        private static bool SkipFile = false;

        /// <summary>
        /// Parses a demo than reads the created json file.
        /// </summary>
        /// <param name="demopath"></param>
        public static void ParseDemoAndReadFile(string demopath)
        {
            ParseDemoFile(demopath, DEFAULT_TASK);
            ReadGamestateJSONFile(demopath, DEFAULT_TASK);
        }

        /// <summary>
        /// Just parses a demo file from the current game with a given parsetasksetting and creates a json.
        /// </summary>
        /// <param name="demopath"></param>
        public static void ParseDemoFile(string demopath, ParseTaskSettings ptask)
        {
            Console.WriteLine("Reading: " + Path.GetFileName(demopath));
            try
            {
                var jsonpath = demopath.Replace(".dem", ".json");
                if (File.Exists(jsonpath))
                {
                    Console.WriteLine(".dem file already parsed");
                }
                else
                {
                    using (var demoparser = new DemoParser(File.OpenRead(demopath)))
                    {
                        Generator = new CSGOGameStateGenerator(demoparser, ptask);
                        SkipFile = CheckForSkip();
                        if (!SkipFile)
                        {
                            Console.WriteLine("Parsing .dem file");
                            Generator.GenerateJSONFile();
                            Generator.CleanUp();
                        }
                        else
                        {
                            Console.WriteLine("----- Not supported. Skip file: " + demopath + "-----");
                            return;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("----- Error occured. Skip file: " + demopath + "-----");
                return;
            }

            Console.WriteLine("----- Parsing demo to GamestateJSON was sucessful ----- ");
        }

        /// <summary>
        /// Read a supported replay file or parse if a demo file is given
        /// </summary>
        /// <param name="demopath"></param>
        private static EncounterDetectionReplay ReadGamestateJSONFile(string demopath, ParseTaskSettings ptask)
        {
            Console.WriteLine("Reading: " + Path.GetFileName(demopath));
            try
            {
                var jsonpath = demopath.Replace(".dem", ".json");
                if (File.Exists(jsonpath) || Path.GetExtension(demopath) != ".json")
                {
                    if (new FileInfo(jsonpath).Length == 0)
                    {
                        Console.WriteLine("----- File empty -----");
                        return null; //File was empty -> skipped parsing it but wrote json
                    }
                    Console.WriteLine("----- .dem file already parsed -----");
                    Console.WriteLine("----- Reading: " + jsonpath + " for Encounter Detection -----");

                    using (var reader = new StreamReader(jsonpath))
                    {
                        var deserializedGamestate = JsonConvert.DeserializeObject<ReplayGamestate>(reader.ReadToEnd(), ptask.Settings);
                        Console.WriteLine("Map: " + deserializedGamestate.Meta.Mapname);
                        if (deserializedGamestate == null)
                            throw new Exception("Gamestate null");

                        string metapath = META_PATH + deserializedGamestate.Meta.Mapname + ".txt";
                        var mapmeta = MapMetaDataPropertyReader.ReadProperties(metapath);
                        Console.WriteLine("Detecting Encounters");
                        Console.WriteLine("----- Reading GamestateJSON was sucessful ----- ");
                        return new EncounterDetection(deserializedGamestate, mapmeta, new CSGOPreprocessor()).DetectEncounters();
                    }
                }
                else
                {
                    Console.WriteLine("----- File not existent or right format: " + demopath + "-----");
                    return null;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("----- Error occured. Skip file: " + demopath + "-----");
                return null;
            }

        }

        /// <summary>
        /// Read a demo json file if the corresponding replay file was already parsed 
        /// </summary>
        /// <param name="jsonpath"></param>
        /// <param name="ptask"></param>
        public static EncounterDetection ReadDemoJSON(string jsonpath, ParseTaskSettings ptask)
        {
            Console.WriteLine("Reading: " + jsonpath + " for Encounter Detection");

            using (var reader = new StreamReader(jsonpath))
            {
                var deserializedGamestate = JsonConvert.DeserializeObject<ReplayGamestate>(reader.ReadToEnd(), ptask.Settings);
                Console.WriteLine("Map: " + deserializedGamestate.Meta.Mapname);
                if (deserializedGamestate == null)
                    throw new Exception("Gamestate null");

                string metapath = META_PATH + deserializedGamestate.Meta.Mapname + ".txt";
                var mapmeta = MapMetaDataPropertyReader.ReadProperties(metapath);
                mapmeta.Mapname = deserializedGamestate.Meta.Mapname;
                Console.WriteLine(mapmeta);
                Console.WriteLine("Detecting Encounters");
                return new EncounterDetection(deserializedGamestate, mapmeta, new CSGOPreprocessor());
            }
        }

        /// <summary>
        /// Skip a file because it is not supported or has other errors
        /// </summary>
        /// <param name="demoparser"></param>
        /// <param name="ptask"></param>
        /// <returns></returns>
        private static bool CheckForSkip()
        {
            //Check for supported map
            var mapname = Generator.PeakMapname();
            Console.WriteLine("Map: " + mapname);
            if (!Map.SUPPORTED_MAPS.Contains(mapname))
                return true;
            return false;
        }
    }
}
