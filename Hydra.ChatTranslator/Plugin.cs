using Hydra.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Hydra.ChatTranslator
{

    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override Version Version => new Version(1, 0, 1, 0);

        public override string Name
        {
            get { return "Hydra.ChatTranslator"; }
        }

        public override string Author
        {
            get { return "Vednix"; }
        }

        public Plugin(Main game) : base(game)
        {
            Order = 1;
        }
        internal static bool Wait = false;
        internal static bool[] DisableTr = new bool[Main.maxPlayers];
        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
            ServerApi.Hooks.GamePostInitialize.Register(this, Config.OnPluginInitialize);
            GeneralHooks.ReloadEvent += Config.OnReloadEvent;
            TShockAPI.Commands.ChatCommands.Add(new Command(TranslatorToggle, "tradutor", "translator", "traductor", "google", "tr", "googletr", "trad")
            {
                AllowServer = false
            });
        }
        private static void TranslatorToggle(CommandArgs args)
        {
            DisableTr[args.Player.Index] = !DisableTr[args.Player.Index];
            TSPlayerB.SendSuccessMessage(args.Player.Index, DefaultMessage: string.Format("Now you {0} receive messages translated from other languages", DisableTr[args.Player.Index] ? "[c/98C807:will]" : "[c/ffa500:will not]"),
                                                            PortugueseMessage: string.Format("Agora você {0} receber mensagens traduzidas de outros idiomas", DisableTr[args.Player.Index] ? "[c/98C807:irá]" : "[c/ffa500:não irá]"),
                                                            SpanishMessage: string.Format("Ahora {0} mensajes traducidos de otros idiomas.", DisableTr[args.Player.Index] ? "[c/98C807:recibirá]" : "[c/ffa500:no recibirá]"));
        }
        public static Config PConfig;
        internal static async void OnServerChat(ServerChatEventArgs args)
        {
            if (args.Handled)
                return;

            var tsplr = TShockB.Players[args.Who];
            if (tsplr == null)
            {
                args.Handled = true;
                return;
            }

            if (args.Text.Length > 500)
            {
                TSPlayerB.SendWarningMessage(tsplr.Index, DefaultMessage: "Crash attempt via long chat packet.",
                                                          PortugueseMessage: "Tentativa de exploit através de um pacote de bate-papo longo",
                                                          SpanishMessage: "Intento de explotación a través de un paquete de chat largo");
                //Utils.Kick(tsplr, "Crash attempt via long chat packet.", true);
                args.Handled = true;
                return;
            }

            string text = args.Text;

            // Terraria now has chat commands on the client side.
            // These commands remove the commands prefix (e.g. /me /playing) and send the command id instead
            // In order for us to keep legacy code we must reverse this and get the prefix using the command id
            //Not in Terraria 1.3.0
            //foreach (var item in Terraria.UI.Chat.ChatManager.Commands._localizedCommands)
            //{
            //	if (item.Value._name == args.CommandId._name)
            //	{
            //		if (!String.IsNullOrEmpty(text))
            //		{
            //			text = item.Key.Value + ' ' + text;
            //		}
            //		else
            //		{
            //			text = item.Key.Value;
            //		}
            //		break;
            //	}
            //}

            if ((text.StartsWith(TShock.Config.CommandSpecifier) || text.StartsWith(TShock.Config.CommandSilentSpecifier)) && !string.IsNullOrWhiteSpace(text.Substring(1)))
                try
                {
                    args.Handled = true;
                    if (!TShockAPI.Commands.HandleCommand(tsplr, text))
                    {
                        // This is required in case anyone makes HandleCommand return false again
                        TSPlayerB.SendErrorMessage(tsplr.Index, DefaultMessage: "The command could not be parsed.",
                                                                PortugueseMessage: "O comando não pôde ser analisado.",
                                                                SpanishMessage: "El comando no se pudo analizar.");

                        Logger.doLogLang(DefaultMessage: $"Unable to parse command '{text}' from player '{tsplr.Name}'.", Hydra.Config.DebugLevel.Error, (TSPlayerB.Language)Enum.Parse(typeof(TSPlayerB.Language), Base.Config.DefaultLanguage),
                                         PortugueseMessage: $"Não foi possível parsear o comando '{text}' executado pelo jogador '{tsplr.Name}'.",
                                         SpanishMessage: $"El comando no se pudo analizar '{text}' realizado por el jugador '{tsplr.Name}'.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.doLogLang(DefaultMessage: $"An exception occurred executing a command.", Hydra.Config.DebugLevel.Critical, (TSPlayerB.Language)Enum.Parse(typeof(TSPlayerB.Language), Base.Config.DefaultLanguage),
                                     PortugueseMessage: $"Ocorreu uma exceção ao executar um comando.",
                                     SpanishMessage: $"Se produjo una excepción al ejecutar un comando.");

                    TShock.Log.Error(ex.ToString());
                }
            else
            {
                if (TShock.Config.EnableChatAboveHeads)
                {
                    TShock.Config.EnableChatAboveHeads = false;
                    Logger.doLog("ChatAboveHeads not yet implemented in Hydra, using default", Hydra.Config.DebugLevel.Critical);
                }
                if (!tsplr.HasPermission(TShockAPI.Permissions.canchat))
                {
                    args.Handled = true;
                }
                else if (tsplr.mute)
                {
                    TSPlayerB.SendErrorMessage(tsplr.Index, DefaultMessage: "You have been muted!",
                                                            PortugueseMessage: "Você foi silenciado!",
                                                            SpanishMessage: "Has sido silenciado!");
                    args.Handled = true;
                }
                else if (!TShock.Config.EnableChatAboveHeads)
                {
                    string PreNSuf = $"{tsplr.Group.Prefix}{tsplr.Name}{tsplr.Group.Suffix}: ";
                    await Broadcast(PreNSuf, args.Text, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B, tsplr, args);

                    //text = String.Format(Config.ChatFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix,
                    //                         args.Text);
                    //Hooks.PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);
                    //Utils.Broadcast(text, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);

                    //var hexValue = tsplr.Group.R.ToString("X2") + tsplr.Group.G.ToString("X2") + tsplr.Group.B.ToString("X2");
                    //string c = Convert.ToInt32(hexValue, 16).ToString();
                    args.Handled = true;
                }
                else
                {
                    Player ply = Main.player[args.Who];
                    string name = ply.name;
                    ply.name = String.Format(TShock.Config.ChatAboveHeadsFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix);
                    //Update the player's name to format text nicely. This needs to be done because Terraria automatically formats messages against our will
                    NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, NetworkText.FromLiteral(ply.name), args.Who, 0, 0, 0, 0);

                    //Give that poor player their name back :'c
                    ply.name = name;
                    PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);

                    NetMessage.SendData((int)PacketTypes.ChatText, -1, args.Who, text, args.Who, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
                    NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, name, args.Who, 0, 0, 0, 0);
                    //Downgrade to 1.3.0
                    ////This netpacket is used to send chat text from the server to clients, in this case on behalf of a client
                    //Terraria.Net.NetPacket packet = Terraria.GameContent.NetModules.NetTextModule.SerializeServerMessage(
                    //    NetworkText.FromLiteral(text), new Color(tsplr.Group.R, tsplr.Group.G, tsplr.Group.B), (byte)args.Who
                    //);
                    ////Broadcast to everyone except the player who sent the message.
                    ////This is so that we can send them the same nicely formatted message that everyone else gets
                    //Terraria.Net.NetManager.Instance.Broadcast(packet, args.Who);

                    //Reset their name
                    //NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, NetworkText.FromLiteral(name), args.Who, 0, 0, 0, 0);

                    string msg = String.Format("<{0}> {1}",
                        String.Format(TShock.Config.ChatAboveHeadsFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix),
                        text
                    ); ;

                    //Send the original sender their nicely formatted message, and do all the loggy things
                    tsplr.SendMessage(msg, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
                    TSPlayer.Server.SendMessage(msg, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
                    TShock.Log.Info("Broadcast: {0}", msg);
                    args.Handled = true;
                }
            }
        }
        internal static string SuccessTag()
        {
            return " [c/b3ef1f:(V)]";
        }
        internal static string ErrorTag()
        {
            return " [c/ff0000:(X)]";
        }
        internal static async Task Broadcast(string PreNSuf, string Message, byte r, byte g, byte b, TSPlayer tsplr, ServerChatEventArgs args)
        {
            args.Handled = true;
            string OriginalMessage = $"{PreNSuf}{Message}";
            TShock.Log.Info(string.Format("Broadcast: {0}", OriginalMessage));
            string text = String.Format(PConfig.ChatFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix, args.Text);

            bool[] PlayerLangOn = new bool[3]; //0 = EN | 1 = PT | 2 = ES
            string[] TrMessage = new string[3]; //0 = EN | 1 = PT | 2 = ES

            if (!PConfig.Enabled)
            {
                Color c = new Color(r, g, b);
                TShockB.AllSendMessage(OriginalMessage, c);
                Logger.WriteLine($"[{Logger.DateTimeNow}] [CHAT] {System.Text.RegularExpressions.Regex.Replace(OriginalMessage, @"\[c\/[a-f0-9]{6}:([^\]]+)]", @"$1")}", ConsoleColor.DarkGreen);
                PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);
                return;
            }

            TSPlayerB.SendMessage(tsplr.Index, OriginalMessage, r, g, b);

            if (!PConfig.ConsoleChatTranslate)
                Logger.WriteLine($"[{Logger.DateTimeNow}] [CHAT] {System.Text.RegularExpressions.Regex.Replace(OriginalMessage, @"\[c\/[a-f0-9]{6}:([^\]]+)]", @"$1")}", ConsoleColor.DarkGreen);

            foreach (var plr in TShockB.Players.Where(p => p != null && p.Active))
            {
                PlayerLangOn[(int)Enum.Parse(typeof(TSPlayerB.Language), Base.Config.DefaultLanguage)] = true;
                if (PlayerLangOn[0] && PlayerLangOn[1] && PlayerLangOn[2])
                    break;
                switch (TSPlayerB.PlayerLanguage[plr.Index])
                {
                    case TSPlayerB.Language.English:
                        PlayerLangOn[0] = true;
                        break;
                    case TSPlayerB.Language.Portuguese:
                        PlayerLangOn[1] = true;
                        break;
                    case TSPlayerB.Language.Spanish:
                        PlayerLangOn[2] = true;
                        break;
                }
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    foreach (var Data in PConfig.WordsReplaces)
                        Message = System.Text.RegularExpressions.Regex.Replace(Message, $@"\b{Data.From}\b", Data.To);

                    var translator = new GoogleTranslateFreeApi.GoogleTranslator();

                    GoogleTranslateFreeApi.Language from = GoogleTranslateFreeApi.Language.Auto;

                    for (int i = 0; i < PlayerLangOn.Count(); i++)
                    {
                        if (i == (int)TSPlayerB.PlayerLanguage[tsplr.Index])
                        {
                            TrMessage[i] = $"{OriginalMessage}{SuccessTag()}";
                            continue;
                        }
                        if (PlayerLangOn[i])
                        {
                            GoogleTranslateFreeApi.Language to = GoogleTranslateFreeApi.Language.English;
                            switch (i)
                            {
                                case 1:
                                    to = GoogleTranslateFreeApi.Language.Portuguese;
                                    break;
                                case 2:
                                    to = GoogleTranslateFreeApi.Language.Spanish;
                                    break;
                            }
                            var result = await translator.TranslateLiteAsync(Message, from, to);
                            TrMessage[i] = $"{PreNSuf}{result.MergedTranslation}{SuccessTag()}";
                        }
                    }
                    if (PConfig.ConsoleChatTranslate)
                        Logger.WriteLine($"[{Logger.DateTimeNow}] [CHAT] {System.Text.RegularExpressions.Regex.Replace(TrMessage[(int)Enum.Parse(typeof(TSPlayerB.Language), Base.Config.DefaultLanguage)], @"\[c\/[a-f0-9]{6}:([^\]]+)]", @"$1")}", ConsoleColor.DarkGreen);
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                for (int i = 0; i < PlayerLangOn.Count(); i++)
                    if (PlayerLangOn[i])
                        TrMessage[i] = $"{OriginalMessage}{ErrorTag()}";
                if (PConfig.ConsoleChatTranslate)
                    Logger.WriteLine($"[{Logger.DateTimeNow}] [CHAT] {System.Text.RegularExpressions.Regex.Replace(TrMessage[(int)Enum.Parse(typeof(TSPlayerB.Language), Base.Config.DefaultLanguage)], @"\[c\/[a-f0-9]{6}:([^\]]+)]", @"$1")}", ConsoleColor.DarkRed);
                Logger.doLog($"[Hydra.ChatTranslator] {ex.Message}", Hydra.Config.DebugLevel.Error);
            }

            Parallel.ForEach(TShock.Players.Where(p => p != null && p != tsplr && p.Active), fchplr =>
            {
                if (DisableTr[fchplr.Index])
                    TSPlayerB.SendMessage(fchplr.Index, OriginalMessage, r, g, b);
                else if (TSPlayerB.PlayerLanguage[tsplr.Index] == TSPlayerB.PlayerLanguage[fchplr.Index])
                    TSPlayerB.SendMessage(fchplr.Index, TrMessage[(int)TSPlayerB.PlayerLanguage[fchplr.Index]], r, g, b);
                else if (TSPlayerB.PlayerLanguage[tsplr.Index] != TSPlayerB.PlayerLanguage[fchplr.Index])
                    TSPlayerB.SendMessage(fchplr.Index, TrMessage[(int)TSPlayerB.PlayerLanguage[fchplr.Index]], r, g, b);
            });


            //if (Config.ConsoleChatTranslate)
            //    //Logger.WriteLine($"[{Logger.DateTimeNow}] [CHAT] {TrMessage[(int)Enum.Parse(typeof(TSPlayerB.Language), Base.Config.DefaultLanguage)]}", ConsoleColor.Green);
            //    Logger.WriteLine($"[{Logger.DateTimeNow}] [CHAT] {System.Text.RegularExpressions.Regex.Replace(TrMessage[(int)Enum.Parse(typeof(TSPlayerB.Language), Base.Config.DefaultLanguage)], @"\[c\/[a-f0-9]{6}:([^\]]+)]", @"$1")}", ConsoleColor.DarkGreen);

            PlayerHooks.OnPlayerChat(tsplr, args.Text, ref text);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this, OnServerChat);
                GeneralHooks.ReloadEvent -= Config.OnReloadEvent;
                TShockAPI.Commands.ChatCommands.Remove(new Command(TranslatorToggle));
            }
            base.Dispose(disposing);
        }
    }
}
