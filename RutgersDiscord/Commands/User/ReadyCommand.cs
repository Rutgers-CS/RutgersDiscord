using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
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

        public ReadyCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, DatHostAPIHandler datHostAPIHandler)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
            _datHostAPIHandler = datHostAPIHandler;
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
            if(DateTime.UtcNow - t > TimeSpan.FromMinutes(15))
            {
                await _context.Interaction.RespondAsync("Cannot ready more than 15 mins before match", ephemeral: true);
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
                }
            }

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

            //Start match (generate match)
            var newServer = await _datHostAPIHandler.CreateNewServer();
            
            ServerTokens newToken = await _database.GetUnusedToken();
            newToken.ServerID = newServer.ServerID;
            await _database.UpdateToken(newToken);
            await _datHostAPIHandler.UpdateServerToken(newServer.ServerID, newToken.Token);

            TeamInfo homeTeam = await _database.GetTeamAsync((int)match.TeamHomeID);
            PlayerInfo hP1 = await _database.GetPlayerAsync((int)homeTeam.Player1);
            PlayerInfo hP2 = await _database.GetPlayerAsync((int)homeTeam.Player2);
            TeamInfo awayTeam = await _database.GetTeamAsync((int)match.TeamAwayID);
            PlayerInfo aP1 = await _database.GetPlayerAsync((int)awayTeam.Player1);
            PlayerInfo aP2 = await _database.GetPlayerAsync((int)awayTeam.Player2);
            MapInfo map = await _database.GetMapAsync((int)match.MapID);
            MatchSettings ms = new MatchSettings(map, homeTeam, hP1, hP2, awayTeam, aP1, aP2, newServer.ServerID);
            
            await _datHostAPIHandler.CreateMatch(ms);

            var builder = new ComponentBuilder()
                .WithButton("Connect", "connect_btn", style: ButtonStyle.Link, url: $"connect {newServer.IP}:{newServer.Port}");

            await _context.Channel.SendMessageAsync(components: builder.Build());
        }
    }
}
