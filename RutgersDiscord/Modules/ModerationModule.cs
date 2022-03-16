using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Pagination;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RutgersDiscord.Modules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractivityService _interactivity;
        private readonly IServiceProvider _services;
        private readonly DatabaseHandler _database;

        public ModerationModule(DiscordSocketClient client, InteractivityService interactivity, IServiceProvider services, DatabaseHandler database)
        {
            _client = client;
            _interactivity = interactivity;
            _services = services;
            _database = database;
        }

        [SlashCommand("database", "queries database.", runMode: RunMode.Async)]
        public async Task Database(string query)
        {
            var response = _database.GetTable<string>(query);
            await RespondAsync(response.First());
        }

        [SlashCommand("creatematch", "Creates a match.", runMode: RunMode.Async)]
        public async Task CreateMatch(long teamHomeID, long teamAwayID, int month, int day, int hour)
        {
            DateTime t = new DateTime(DateTime.Now.Year,month,day,hour,0,0);
            GenerateMatches g = new(_client, Context, _database, _interactivity);
            await g.CreateMatch(teamHomeID, teamAwayID,t);
        }

        [SlashCommand("match", "edits matches.", runMode: RunMode.Async)]
        public async Task Match(OperationType op, [ComplexParameter] MatchInfo match)
        {
            switch (op)
            {
                case OperationType.add:
                    if (_database.GetMatchById(match.ID) != null)
                    {
                        await RespondAsync("Match already exists", ephemeral: true);
                        return;
                    }
                    _database.AddMatch(match);
                    await RespondAsync($"match added");
                    break;
                case OperationType.delete:
                    _database.DeleteMatch(match);
                    await RespondAsync($"match deleted");
                    break;
                case OperationType.edit:
                    _database.ModifyMatch(MatchInfo.Merge(_database.GetMatchById(match.ID), match));
                    await RespondAsync($"match edited");
                    break;
            }
        }

        [SlashCommand("test1", "temp", runMode: RunMode.Async)]
        public async Task Temp()
        {
        }
    }
}
