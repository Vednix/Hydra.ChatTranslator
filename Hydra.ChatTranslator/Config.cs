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

            public string From { get; set; }
            public string To { get; set; }
        }
        public bool Enabled = true;
        public string ChatFormat = "{1}{2}{3}: {4}";
        public bool ConsoleChatTranslate = true;
        public List<Data> WordsReplaces { get; set; } = new List<Data>
        {
            new Data{ From = "n", To = "não" }
        };
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

                Plugin.PConfig = config;

                Logger.doLogLang(DefaultMessage: $"Configuration has been loaded successfully!", Hydra.Config.DebugLevel.Info, Base.CurrentHydraLanguage, Plugin._name,
                                 PortugueseMessage: $"A configuração foi carregada com sucesso!",
                                 SpanishMessage: $"¡La configuración se ha cargado correctamente!");
            }
            catch (Exception ex)
            {
                Plugin.PConfig = new Config();
                Logger.doLogLang(DefaultMessage: $"There was a critical error loading the Hydra configuration file, using default configuration. => {ex}!", Hydra.Config.DebugLevel.Error, Base.CurrentHydraLanguage, Plugin._name,
                                 PortugueseMessage: $"Ocorreu um erro ao carregar o arquivo de configuração, usando configurações padrões. => {ex}",
                                 SpanishMessage: $"Se produjo un error crítico al cargar el archivo de configuración de Hydra, utilizando la configuración predeterminada. => {ex}");
                Return = false;
            }
            return Return;
        }
    }
}
