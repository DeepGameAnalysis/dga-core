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
        private const string META_PATH = @"D:\Ressources\CS GO Demofiles\CS GO Mapmetadata\";
        private static List<string> invalidfiles = new List<string>();

        /// <summary>
        /// Read all supported replay files at the given path
        /// </summary>
        /// <param name="PATH"></param>
        public static void ReadAllFiles(string PATH)
        {
            var filelist = Directory.EnumerateFiles(PATH, "*.dem");
            Console.WriteLine("Files to handle: " + filelist.Count());
            foreach (string file in filelist)
                ReadDemoFile(file);

            foreach (string invalidfile in invalidfiles)
                Console.WriteLine("Could not parse: " + invalidfile + "\nReplay not supported yet. Please use only dust2");

        }

        /// <summary>
        /// Read all files given through the command line
        /// </summary>
        /// <param name="args"></param>
        public static void ReadFilesFromCommandline(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
                ReadDemoFile(args[i]);
        }

        private static bool skipfile = false;


        /// <summary>
        /// Read a supported replay file
        /// </summary>
        /// <param name="demopath"></param>
        private static void ReadDemoFile(string demopath)
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
                settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented }
            };

            Console.WriteLine("Reading: " + Path.GetFileName(demopath));
            try
            {
                var jsonpath = demopath.Replace(".dem", ".json");
                if (File.Exists(jsonpath))
                {
                    if (new FileInfo(jsonpath).Length == 0)
                    {
                        Console.WriteLine("File empty");
                        return; //File was empty -> skipped parsing it but wrote json
                    }
                    Console.WriteLine(".dem file already parsed");
                    ReadDemoJSON(jsonpath, ptask);
                }
                else
                {
                    using (var demoparser = new DemoParser(File.OpenRead(demopath)))
                    {
                        skipfile = SkipFile(demoparser, ptask);
                        if (!skipfile)
                        {
                            Console.WriteLine("Parsing .dem file");
                            using (var newdemoparser = new DemoParser(File.OpenRead(demopath)))
                            {
                                CSGOGameStateGenerator.GenerateJSONFile(newdemoparser, ptask);
                                ReadDemoJSON(jsonpath, ptask);
                                CSGOGameStateGenerator.cleanUp();
                            }
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

            Console.WriteLine("----- Parsing and Encounter Detection was sucessful ----- ");

        }

        /// <summary>
        /// Read a demo json file if the corresponding replay file was already parsed 
        /// </summary>
        /// <param name="jsonpath"></param>
        /// <param name="ptask"></param>
        private static void ReadDemoJSON(string jsonpath, ParseTask ptask)
        {
            Console.WriteLine("Reading: " + jsonpath + " for Encounter Detection");

            using (var reader = new StreamReader(jsonpath))
            {
                var deserializedGamestate = Newtonsoft.Json.JsonConvert.DeserializeObject<ReplayGamestate>(reader.ReadToEnd(), ptask.settings);
                Console.WriteLine("Map: " + deserializedGamestate.meta.mapname);
                if (deserializedGamestate == null)
                    throw new Exception("Gamestate null");

                string metapath = META_PATH + deserializedGamestate.meta.mapname + ".txt";
                var mapmeta = MapMetaDataPropertyReader.readProperties(metapath);
                Console.WriteLine("Detecting Encounters");
                new EncounterDetection(deserializedGamestate, mapmeta, new CSGOPreprocessor()).DetectEncounters();
            }
        }

        /// <summary>
        /// Skip a file because it is not supported or has other errors
        /// </summary>
        /// <param name="demoparser"></param>
        /// <param name="ptask"></param>
        /// <returns></returns>
        private static bool SkipFile(DemoParser demoparser, ParseTask ptask)
        {
            var mapname = CSGOGameStateGenerator.peakMapname(demoparser, ptask);
            Console.WriteLine("Map: " + mapname);
            if (!Map.SUPPORTED_MAPS.Contains(mapname))
                return true;

            CSGOGameStateGenerator.cleanUp();
            return false;
        }
    }
}
