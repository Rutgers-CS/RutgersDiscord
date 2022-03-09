using System;
using Interactivity;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using RutgersDiscord.Handlers;

namespace RutgersDiscord
{
    class Program
    {
        private static DiscordSocketClient _client;
        private static IServiceProvider _services;
        private static InteractionService _interaction;

        static async Task Main(string[] args)
        {
            var interactiveConfig = new InteractivityConfig
            {
                DefaultTimeout = TimeSpan.FromMinutes(10)
            };
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(config);
            _interaction = new InteractionService(_client.Rest);
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_interaction)
                .AddSingleton<InteractionHandler>()
                .AddSingleton<DatabaseHandler>()
                .AddSingleton(s => new InteractivityService(_client, interactiveConfig))
                .BuildServiceProvider();

            await _services.GetRequiredService<InteractionHandler>().InstallAsync();

            _client.Ready += ClientReady;

            string token = Environment.GetEnvironmentVariable("botToken"); //DBPass
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);

        }

        static public async Task ClientReady()
        {
#if DEBUG
            ulong localDiscordServer = ulong.Parse(Environment.GetEnvironmentVariable("discordTestServer"));
            await _interaction.RegisterCommandsToGuildAsync(localDiscordServer);
#else
            await _interaction.RegisterCommandsGloballyAsync();
#endif
        }
    }
}
