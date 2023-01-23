using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherReport.Commands;

namespace WeatherReport {
    public class Bot {
        public DiscordClient client { get; private set; }
        public CommandsNextExtension commands { get; private set; }
        public InteractivityExtension interactivity { get; private set; } 
        public async Task RunAsync() {

            //Loading Json with Token and Prefix
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            json = await sr.ReadToEndAsync().ConfigureAwait(false);
            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            //Configurate Discord Client
            var config = new DiscordConfiguration {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
            };

            //Confiruge Commands
            var commandsConfig = new CommandsNextConfiguration {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true
            };

            //Set up buttons
            var leftButton = new DiscordButtonComponent(ButtonStyle.Secondary, "leftButton", "", false, new DiscordComponentEmoji("\U00002B05"));
            var rightButton = new DiscordButtonComponent(ButtonStyle.Secondary, "rightButton", "", false ,new DiscordComponentEmoji("\U000027A1"));
            
            //Launch Discord Client
            client = new DiscordClient(config);
            client.Ready += OnCilentReady;

            //Confiruge Interactivity
            client.UseInteractivity(new InteractivityConfiguration {
                PaginationButtons = new DSharpPlus.Interactivity.EventHandling.PaginationButtons {
                    Left = leftButton,
                    Right = rightButton,
                },
                PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.Ignore,
                AckPaginationButtons = true
            });

            //Inicialize Commands
            commands = client.UseCommandsNext(commandsConfig);
            commands.RegisterCommands<WeatherCommands>();  

            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task OnCilentReady(DiscordClient client, ReadyEventArgs e) {
            Console.WriteLine("Bot ready");
            return Task.CompletedTask;
        }
    }
}
