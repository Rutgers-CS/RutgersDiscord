using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using Discord.Rest;

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
            var response = await _database.GetTable<string>(query);
            await RespondAsync(response.First());
        }

        [SlashCommand("creatematch", "Creates a match.", runMode: RunMode.Async)]
        public async Task CreateMatch(int teamHomeID, int teamAwayID, int month, int day, int hour)
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
                case OperationType.create:
                    if(_database.GetMatchAsync(match.MatchID) != null)
                    {
                        await RespondAsync("Match already exists", ephemeral: true);
                        return;
                    }
                    await _database.AddMatchAsync(match);
                    await RespondAsync($"match added");
                    break;
                case OperationType.delete:
                    await _database.DeleteMatchAsync(match.MatchID);
                    await RespondAsync($"match deleted");
                    break;
                case OperationType.update:
                    await _database.UpdateMatchAsync(MatchInfo.Merge(_database.GetMatchAsync(match.MatchID).Result, match));
                    await RespondAsync($"match edited");
                    break;
            }
        }

        #region Manual Database Commands
        #region Players
        [SlashCommand("create-player", "Creates a player", runMode: RunMode.Async)]
        public async Task CreatePlayer([ComplexParameter] PlayerInfo player)
        {
            await _database.AddPlayerAsync(player);
            await RespondAsync("Player Created");
        }

        [SlashCommand("read-player", "Reads a player", runMode: RunMode.Async)]
        public async Task ReadPlayer(long playerID)
        {
            PlayerInfo player = await _database.GetPlayerAsync(playerID);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithDescription(player.ToString());

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("update-player", "Updates a player", runMode: RunMode.Async)]
        public async Task UpdatePlayer([ComplexParameter] PlayerInfo player)
        {
            await _database.UpdatePlayerAsync(player);
            await RespondAsync("Player Updated");
        }

        [SlashCommand("delete-player", "Deletes a player", runMode: RunMode.Async)]
        public async Task DeletePlayer(long playerID)
        {
            await _database.DeletePlayerAsync(playerID);
            await RespondAsync("Player Deleted");
        }

        [SlashCommand("list-players", "Lists all players", runMode: RunMode.Async)]
        public async Task ListAllPlayers()
        {
            var players = await _database.GetAllPlayersAsync();
            List<PlayerInfo> playerList = players.ToList();
            string output = "";
            foreach (var player in players)
            {
                output = output + player.ToString() + "\n\n";
            }
            await RespondAsync(output);
        }
        #endregion

        #region Teams
        [SlashCommand("create-team", "Creates a team", runMode: RunMode.Async)]
        public async Task CreateTeam([ComplexParameter] TeamInfo team)
        {
            await _database.AddTeamAsync(team);
            await RespondAsync("Team Created");
        }

        [SlashCommand("read-team", "Reads a team", runMode: RunMode.Async)]
        public async Task ReadTeam(long teamID)
        {
            TeamInfo team = await _database.GetTeamAsync(teamID);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithDescription(team.ToString());

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("update-team", "Updates a team", runMode: RunMode.Async)]
        public async Task UpdateTeam([ComplexParameter] TeamInfo team)
        {
            await _database.UpdateTeamAsync(team);
            await RespondAsync("Team Updated");
        }

        [SlashCommand("delete-team", "Deletes a team", runMode: RunMode.Async)]
        public async Task DeleteTeam(long teamID)
        {
            await _database.DeleteTeamAsync(teamID);
            await RespondAsync("Team Deleted");
        }

        [SlashCommand("list-teams", "Lists all teams", runMode: RunMode.Async)]
        public async Task ListAllTeams()
        {
            var teams = await _database.GetAllTeamsAsync();
            string output = "";
            foreach (var team in teams)
            {
                output = output + team.ToString() + "\n\n";
            }

            await RespondAsync(output);
        }
        #endregion

        #region Matches
        [SlashCommand("create-match", "Creates a match", runMode: RunMode.Async)]
        public async Task CreateMatch([ComplexParameter] MatchInfo match)
        {
            await _database.AddMatchAsync(match);
            await RespondAsync("Match Created");
        }

        [SlashCommand("read-match", "Reads a match", runMode: RunMode.Async)]
        public async Task ReadMatch(long matchID)
        {
            MatchInfo match = await _database.GetMatchAsync(matchID);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithDescription(match.ToString());

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("update-match", "Updates a match", runMode: RunMode.Async)]
        public async Task UpdateMatch([ComplexParameter] MatchInfo match)
        {
            await _database.UpdateMatchAsync(match);
            await RespondAsync("Match Updated");
        }

        [SlashCommand("delete-match", "Deletes a match", runMode: RunMode.Async)]
        public async Task DeleteMatch(long matchID)
        {
            await _database.DeleteMatchAsync(matchID);
            await RespondAsync("Match Deleted");
        }

        [SlashCommand("list-matches", "Lists all matches", runMode: RunMode.Async)]
        public async Task ListAllMatches()
        {
            var matches = await _database.GetAllMatchesAsync();
            string output = "";
            foreach (var match in matches)
            {
                output = output + match.ToString() + "\n\n";
            }
            await RespondAsync(output);
        }
        #endregion

        #region Maps
        [SlashCommand("create-map", "Creates a map", runMode: RunMode.Async)]
        public async Task CreateMap([ComplexParameter] MapInfo map)
        {
            await _database.AddMapAsync(map);
            await RespondAsync("Map Created");
        }

        [SlashCommand("read-map", "Reads a map", runMode: RunMode.Async)]
        public async Task ReadMap(long mapID)
        {
            MapInfo map = await _database.GetMapAsync(mapID);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithDescription(map.ToString());

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("update-map", "Updates a map", runMode: RunMode.Async)]
        public async Task UpdateMap([ComplexParameter] MapInfo map)
        {
            await _database.UpdateMapAsync(map);
            await RespondAsync("Map Updated");
        }

        [SlashCommand("delete-map", "Deletes a map", runMode: RunMode.Async)]
        public async Task DeleteMap(long mapID)
        {
            await _database.DeleteMapAsync(mapID);
            await RespondAsync("Map Deleted");
        }

        [SlashCommand("list-maps", "Lists all maps", runMode: RunMode.Async)]
        public async Task ListAllMaps()
        {
            var maps = await _database.GetAllMapsAsync();
            string output = "";
            foreach (var map in maps)
            {
                output = output + map.ToString() + "\n\n";
            }
            await RespondAsync(output);
        }
        #endregion

        [SlashCommand("test-data", "adds test data to database", runMode: RunMode.Async)]
        public async Task TestData()
        {
            await _database.AddTestData();
            await RespondAsync("Test Data Added");
        }
        #endregion


        [SlashCommand("temp", "temp", runMode: RunMode.Async)]
        public async Task Temp()
        {
            var a = _client.GetGuild(Constants.guild).GetTextChannel(Context.Channel.Id).SendMessageAsync("test");
            //await channel.SendMessageAsync("test");
            await DeferAsync();
        }


    }
}
