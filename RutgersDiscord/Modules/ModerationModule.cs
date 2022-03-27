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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using RutgersDiscord.Commands.Admin;

namespace RutgersDiscord.Modules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractivityService _interactivity;
        private readonly DatabaseHandler _database;
        private readonly ScheduleHandler _schedule;
        private readonly RegistrationHandler _registrationHandler;
        private readonly DatHostAPIHandler _datHostAPIService;
        private readonly ConfigHandler _config;

        public ModerationModule(DiscordSocketClient client, InteractivityService interactivity, DatabaseHandler database, ScheduleHandler schedule, RegistrationHandler registrationHandler, DatHostAPIHandler datHostAPIService, ConfigHandler config)
        {
            _client = client;
            _interactivity = interactivity;
            _database = database;
            _schedule = schedule;
            _registrationHandler = registrationHandler;
            _datHostAPIService = datHostAPIService;
            _config = config;
        }

        [SlashCommand("db-query", "queries database.", runMode: RunMode.Async)]
        public async Task DBQuery(string query)
        {
            var response = await _database.GetTable<string>(query);
            string output = "";
            int split = 0;
            foreach (dynamic r in response)
            {
                output += r + "\n";
                if (split == 9)
                {
                    await Context.Channel.SendMessageAsync(output);
                    output = "";
                    split = 0;
                }
                split++;
            }
            await RespondAsync(output);
        }

        [SlashCommand("db-execute", "executes on database.", runMode: RunMode.Async)]
        public async Task DBExecute(string query)
        {
            var response = await _database.GenExec(query);
            await RespondAsync(response.ToString());
        }

        [SlashCommand("db-backup", "backs-up database. !!blocks other connections!!", runMode: RunMode.Async)]
        public async Task DBBackup()
        {
            string filepath = await _database.BackupDatabase();
            await RespondWithFileAsync(new FileAttachment($"./{filepath}"));
        }

        /*        [SlashCommand("announcement", "Posts announcement", runMode: RunMode.Async)]
                public async Task PostAnnouncement()
                {
                    PostAnnouncement pa = new PostAnnouncement(_client, Context, _database, _interactivity);
                    await pa.GetAnnouncement();
                }*/

        /*        [SlashCommand("database", "queries database.", runMode: RunMode.Async)]
                public async Task Database(string query)
                {
                    var response = await _database.GetTable<string>(query);
                    string output = "";
                    int split = 0;
                    foreach (dynamic r in response)
                    {
                        output += r + "\n";
                        if (split == 9)
                        {
                            await Context.Channel.SendMessageAsync(output);
                            output = "";
                            split = 0;
                        }
                        split++;
                    }
                    await RespondAsync(output);
                }*/

/*        [SlashCommand("creatematch", "Creates a match.", runMode: RunMode.Async)]*/
        public async Task CreateMatch(int teamHomeID, int teamAwayID, int month, int day, int hour)
        {
            DateTime t = new DateTime(DateTime.Now.Year,month,day,hour,0,0);
            GenerateMatches g = new(_client, Context, _database, _interactivity,_schedule, _config);
            await g.CreateMatch(teamHomeID, teamAwayID,t);
        }

        [SlashCommand("cm", "Creates a match with string.", runMode: RunMode.Async)]
        public async Task CreateMatchString(string input)
        {
            string[] inputArr = input.Split("    ");
            try
            {
                await CreateMatch(int.Parse(inputArr[0]), int.Parse(inputArr[1]), int.Parse(inputArr[2]), int.Parse(inputArr[3]), int.Parse(inputArr[4]));
            }
            catch
            {
                await Context.Interaction.RespondAsync("Parse failed", ephemeral: true);
            }
        }

        /*[SlashCommand("match", "edits matches.", runMode: RunMode.Async)]
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
                    await _database.UpdateMatchAsync(MatchInfo.Merge((await _database.GetMatchAsync(match.MatchID)), match));
                    await RespondAsync($"match edited");
                    break;
            }
        }*/

/*        [SlashCommand("regisbutton", "creates a reg button", runMode: RunMode.Async)]
        public async Task RegButton()
        {
            var builder = new ComponentBuilder()
                .WithButton("Register Today", "spawn_registration_form", emote: new Emoji("▶"), style: ButtonStyle.Success);
            await RespondAsync(components: builder.Build());
        }*/


/*        #region Manual Database Commands
        #region Players
        [SlashCommand("create-player", "Creates a player", runMode: RunMode.Async)]
        public async Task CreatePlayer([ComplexParameter] PlayerInfo player)
        {
            await _database.AddPlayerAsync(player);
            await RespondAsync("Player Created");
        }

        [SlashCommand("read-player", "Reads a player", runMode: RunMode.Async)]
        public async Task ReadPlayer(string playerID)
        {
            PlayerInfo player = await _database.GetPlayerAsync(long.Parse(playerID));
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
            var player = await _database.GetPlayerAsync(playerID);
            await _database.DeletePlayerAsync(player);
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
        public async Task ReadTeam(int teamID)
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
        public async Task DeleteTeam(int teamID)
        {
            var team = await _database.GetTeamAsync(teamID);
            await _database.DeleteTeamAsync(team);
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
        public async Task ReadMatch(int matchID)
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
        public async Task DeleteMatch([ComplexParameter] MatchInfo match)
        {
            await _database.DeleteMatchAsync(match);
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
        public async Task ReadMap(int mapID)
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
        public async Task DeleteMap(int mapID)
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
        #endregion*/

        [SlashCommand("resolve", "resolves admin call", runMode: RunMode.Async)]
        public async Task Resolve(string matchID)
        {
            //TODO switch to button in NotifyAdmin class
            var match = await _database.GetMatchAsync(int.Parse(matchID));
            match.AdminCalled = false;
            await _database.UpdateMatchAsync(match);
            await RespondAsync("Issue marked resolved");
        }

/*        [SlashCommand("create-server", "creates new server", runMode: RunMode.Async)]
        public async Task CreateServer()
        {
            string rs = (await _datHostAPIService.CreateNewServer()).ToString();
            await RespondAsync(rs);
        }*/

        [SlashCommand("fixdb","RUN ONCE", runMode: RunMode.Async)]
        public async Task FixDB()
        {
            IEnumerable<PlayerInfo> players = await _database.GetAllPlayersAsync();
            foreach(PlayerInfo p in players)
            {
                if(p.Kills == null)
                {
                    p.Kills = 0;
                }
                if (p.Deaths == null)
                {
                    p.Deaths = 0;
                }
                await _database.UpdatePlayerAsync(p);
            }
            IEnumerable<TeamInfo> teams = await _database.GetAllTeamsAsync();
            foreach(TeamInfo t in teams)
            {
                if(t.Losses == null)
                {
                    t.Losses = 0;
                }
                if(t.Wins == null)
                {
                    t.Wins = 0;
                }
                if(t.RoundLosses == null)
                {
                    t.RoundLosses = 0;
                }
                if(t.RoundWins == null)
                {
                    t.RoundWins = 0;
                }
            }
        }
    }
}
