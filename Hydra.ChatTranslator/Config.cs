using Hydra.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using TShockAPI.Hooks;
using static Hydra.Config;

namespace Hydra.ChatTranslator
{
    public class Config
    {
        public struct Data
        {
            public Data(string from, string to)
            {
                From = from;
                To = to;
            }

            public static string From { get; set; }
            public static string To { get; set; }
        }
        public bool Enabled = true;
        public string ChatFormat = "{1}{2}{3}: {4}";
        public bool ConsoleChatTranslate = true;
        public List<Data> GoogleReplaces { get; set; } = new List<Data>();
        public static void OnReloadEvent(ReloadEventArgs args)
        {
            Read();
        }
        public static void OnPluginInitialize(EventArgs args)
        {
            Read();
        }
        public static bool Read()
        {
            bool Return = false;
            try
            {
                if (!Directory.Exists(Base.SavePath))
                    Directory.CreateDirectory(Base.SavePath);
                string filepath = Path.Combine(Base.SavePath, "ChatTranslator.json");

                Config config = new Config();

                if (File.Exists(filepath))
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filepath));
                else
                    File.WriteAllText(filepath, JsonConvert.SerializeObject(config, Formatting.Indented));

                Plugin.Config = config;

                Logger.doLog("[Hydra.ChatTranslator] Configuration has been loaded successfully!", DebugLevel.Info);
            }
            catch (Exception e)
            {
                Plugin.Config = new Config();
                Logger.doLog($"[Hydra.ChatTranslator] There was an error loading the configuration file, using default configuration. => {e.Message}", DebugLevel.Critical);
                Return = false;
            }
            return Return;
        }
    }
}
