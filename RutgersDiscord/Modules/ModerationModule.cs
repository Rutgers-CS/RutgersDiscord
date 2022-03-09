using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RutgersDiscord.Modules
{
    [RequireUserPermission(Discord.GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractivityService _interactive;
        private readonly IServiceProvider _services;
        private readonly DatabaseHandler _database;

        public ModerationModule(DiscordSocketClient client, InteractivityService interactive, IServiceProvider services, DatabaseHandler database)
        {
            _client = client;
            _interactive = interactive;
            _services = services;
            _database = database;
        }

        [SlashCommand("database", "queries database.", runMode: RunMode.Async)]
        public async Task Database(string query)
        {
            var response = _database.GetTable<string>(query);
            await RespondAsync(response.First());
        }
    }
}
