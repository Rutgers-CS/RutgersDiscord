using System;
using Interactivity;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using RutgersDiscord.Handlers;
using System.Web.Http;
using System.Net.Http;
using System.Text;

namespace RutgersDiscord
{
    class Program
    {
        private static DiscordSocketClient _client;
        private static IServiceProvider _services;
        private static InteractionService _interaction;
        private static ConfigHandler _config;

        static async Task Main(string[] args)
        {
            var interactiveConfig = new InteractivityConfig
            {
                DefaultTimeout = TimeSpan.FromMinutes(10),
                RunOnGateway = false
            };
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(config);
            _client.Log += Log;
            _interaction = new InteractionService(_client.Rest);
            _config = new ConfigHandler();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_interaction)
                .AddSingleton(_config)
                .AddSingleton<InteractionHandler>()
                .AddSingleton<DatabaseHandler>()
                .AddSingleton<ScheduleHandler>()
                .AddSingleton<RegistrationHandler>()
                .AddSingleton<RESTHandler>()
                .AddSingleton<GameServerHandler>()
                .AddHttpClient()
                .AddTransient<DatHostAPIHandler>()
                .AddSingleton(s => new InteractivityService(_client, interactiveConfig))
                .BuildServiceProvider();

            await _services.GetRequiredService<InteractionHandler>().InstallAsync();
            await _services.GetRequiredService<ScheduleHandler>().AddRequiredJobsAsync();
            _services.GetRequiredService<RegistrationHandler>().SubscribeHandlers();

            new Task(() => _services.GetRequiredService<RESTHandler>().Listen()).Start();

            _client.Ready += ClientReady;

            string token = _config.settings.DiscordSettings.BotToken;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);

        }

        static public async Task ClientReady()
        {
            ulong localDiscordServer = _config.settings.DiscordSettings.Guild;
            await _interaction.RegisterCommandsToGuildAsync(localDiscordServer, deleteMissing: true);
            //await _interaction.RegisterCommandsGloballyAsync(deleteMissing: true);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
