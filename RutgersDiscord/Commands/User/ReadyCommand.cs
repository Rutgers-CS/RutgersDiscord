using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using Newtonsoft.Json;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Commands
{
    public class ReadyCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly DatHostAPIHandler _datHostAPIHandler;
        private readonly GameServerHandler _gameServerHandler;
        private readonly ConfigHandler _config;

        public ReadyCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, GameServerHandler gameServerHandler, DatHostAPIHandler datHostAPIHandler, ConfigHandler config)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
            _datHostAPIHandler = datHostAPIHandler;
            _gameServerHandler = gameServerHandler;
            _config = config;
        }

        public async Task Ready()
        {
            MatchInfo match = (await _database.GetMatchByAttribute(discordChannel: (long?)_context.Channel.Id)).FirstOrDefault();
            if(match == null)
            {
                await _context.Interaction.RespondAsync("Match not found", ephemeral: true);
                return;
            }

            DateTime t = new DateTime().AddTicks((long)match.MatchTime);
            if(t - DateTime.Now > TimeSpan.FromMinutes(15))
            {
                await _context.Interaction.RespondAsync("Cannot ready more than 15 mins before match", ephemeral: true);
                Console.WriteLine(t.AddMinutes(16).Ticks);
                return;
            }

            TeamInfo team = await _database.GetTeamByDiscordIDAsync((long)_context.Interaction.User.Id, true);
            if (team == null)
            {
                await _context.Interaction.RespondAsync("User not captain of a team", ephemeral: true);
                return;
            }

            if(match.TeamHomeID == team.TeamID)
            {
                if(match.TeamHomeReady == true)
                {
                    await _context.Interaction.RespondAsync($"{team.TeamName} is already ready!");
                    return;
                }
                //ready and wait for enemy
                if (match.TeamAwayReady == false)
                {
                    match.TeamHomeReady = true;
                    await _database.UpdateMatchAsync(match);
                    await _context.Interaction.RespondAsync($"{team.TeamName} is now ready!");
                    return;
                }
            }
            else
            {
                if (match.TeamAwayReady == true)
                {
                    await _context.Interaction.RespondAsync($"{team.TeamName} is already ready!");
                    return;
                }
                //ready and wait for enemy
                if (match.TeamHomeReady == false)
                {
                    match.TeamAwayReady = true;
                    await _database.UpdateMatchAsync(match);
                    await _context.Interaction.RespondAsync($"{team.TeamName} is now ready!");
                    return;
                }
            }

            match.TeamAwayReady = true;
            await _database.UpdateMatchAsync(match);


            //Test if veto was done and do it if not
            await _context.Interaction.DeferAsync();
            if(match.MapID == null)
            {
                await _context.Channel.SendMessageAsync("Veto not done. Starting veto");
                VetoCommand v = new(_client,_context,_database,_interactivity);
                await v.StartVeto();
            }
            else
            {
                await _context.Channel.SendMessageAsync("Starting Match");
            }

            match = (await _database.GetMatchByAttribute(discordChannel: (long?)_context.Channel.Id)).FirstOrDefault();

            if (match.ServerID == null)
            {
                Console.WriteLine("Creating server");
                //Start match (generate match)
                try
                {
                    ServerInfo newServer = await _datHostAPIHandler.CreateNewServer();
                    ServerTokens newToken = await _database.GetUnusedToken();
                    if (newToken != null)
                    {
                        Console.WriteLine(newToken.Token);
                        newToken.ServerID = newServer.ServerID;
                        await _database.UpdateToken(newToken);
                        await _datHostAPIHandler.UpdateServerToken(newServer.ServerID, _context.Channel.Name, newToken.Token);

                        TeamInfo homeTeam = await _database.GetTeamAsync((int)match.TeamHomeID);
                        PlayerInfo hP1 = await _database.GetPlayerAsync((long)homeTeam.Player1);
                        PlayerInfo hP2 = await _database.GetPlayerAsync((long)homeTeam.Player2);
                        TeamInfo awayTeam = await _database.GetTeamAsync((int)match.TeamAwayID);
                        PlayerInfo aP1 = await _database.GetPlayerAsync((long)awayTeam.Player1);
                        PlayerInfo aP2 = await _database.GetPlayerAsync((long)awayTeam.Player2);
                        MapInfo map = await _database.GetMapAsync((int)match.MapID);
                        string webHook = $"http://{_config.settings.ApplicationSettings.PublicIP}:{_config.settings.ApplicationSettings.Port}/api";
                        MatchSettings ms = new MatchSettings(map, homeTeam, hP1, hP2, awayTeam, aP1, aP2, newServer.ServerID, webHook);

                        var st = await _datHostAPIHandler.CreateMatch(ms);
                        PreGameJson preGameJson = JsonConvert.DeserializeObject<PreGameJson>(st);
                        await _context.Channel.SendMessageAsync($"Paste in csgo console: `connect {newServer.IP}:{newServer.Port}`");
                        await _context.Channel.SendMessageAsync($"If you can't connect try again in a few seconds the server might still be booting up");

                        match.ServerID = newServer.ServerID;
                        match.DatMatchID = preGameJson.id;
                        await _database.UpdateMatchAsync(match);
                    }
                    else
                    {
                        await _context.Channel.SendMessageAsync("No free servers availible");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                await _context.Channel.SendMessageAsync("Server already assigned");
            }

            await _context.Interaction.DeleteOriginalResponseAsync();
        }

        public class Ports
        {
            public int game { get; set; }
            public int gotv { get; set; }
            public object gotv_secondary { get; set; }
            public object query { get; set; }
        }

        public class CsgoSettings
        {
            public int slots { get; set; }
            public string steam_game_server_login_token { get; set; }
            public string rcon { get; set; }
            public string password { get; set; }
            public string maps_source { get; set; }
            public string mapgroup { get; set; }
            public string mapgroup_start_map { get; set; }
            public string workshop_id { get; set; }
            public string workshop_start_map_id { get; set; }
            public string workshop_authkey { get; set; }
            public List<object> autoload_configs { get; set; }
            public string sourcemod_admins { get; set; }
            public List<object> sourcemod_plugins { get; set; }
            public bool enable_gotv { get; set; }
            public bool enable_gotv_secondary { get; set; }
            public bool enable_sourcemod { get; set; }
            public bool enable_csay_plugin { get; set; }
            public string game_mode { get; set; }
            public double tickrate { get; set; }
            public bool pure_server { get; set; }
            public bool insecure { get; set; }
            public bool disable_bots { get; set; }
            public bool private_server { get; set; }
            public bool disable_1v1_warmup_arenas { get; set; }
        }

        public class PreGameJson
        {
            public string id { get; set; }
            public string name { get; set; }
            public object user_data { get; set; }
            public string game { get; set; }
            public string location { get; set; }
            public int players_online { get; set; }
            public List<object> status { get; set; }
            public bool booting { get; set; }
            public object server_error { get; set; }
            public string ip { get; set; }
            public string raw_ip { get; set; }
            public object private_ip { get; set; }
            public object match_id { get; set; }
            public bool on { get; set; }
            public Ports ports { get; set; }
            public bool confirmed { get; set; }
            public int max_disk_usage_gb { get; set; }
            public double cost_per_hour { get; set; }
            public double max_cost_per_hour { get; set; }
            public double month_credits { get; set; }
            public int month_reset_at { get; set; }
            public double max_cost_per_month { get; set; }
            public int subscription_cycle_months { get; set; }
            public string subscription_state { get; set; }
            public int subscription_renewal_failed_attempts { get; set; }
            public object subscription_renewal_next_attempt_at { get; set; }
            public int cycle_months_1_discount_percentage { get; set; }
            public int cycle_months_3_discount_percentage { get; set; }
            public int cycle_months_12_discount_percentage { get; set; }
            public int first_month_discount_percentage { get; set; }
            public bool enable_mysql { get; set; }
            public bool autostop { get; set; }
            public int autostop_minutes { get; set; }
            public bool enable_core_dump { get; set; }
            public bool prefer_dedicated { get; set; }
            public bool enable_syntropy { get; set; }
            public string server_image { get; set; }
            public bool reboot_on_crash { get; set; }
            public object manual_sort_order { get; set; }
            public string mysql_username { get; set; }
            public string mysql_password { get; set; }
            public string ftp_password { get; set; }
            public object disk_usage_bytes { get; set; }
            public object default_file_locations { get; set; }
            public string custom_domain { get; set; }
            public List<object> scheduled_commands { get; set; }
            public object added_voice_server { get; set; }
            public string duplicate_source_server { get; set; }
            public CsgoSettings csgo_settings { get; set; }
            public object mumble_settings { get; set; }
            public object teamfortress2_settings { get; set; }
            public object teamspeak3_settings { get; set; }
            public object valheim_settings { get; set; }
        }

    }
}
