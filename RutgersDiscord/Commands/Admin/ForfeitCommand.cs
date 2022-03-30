using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Commands.Admin
{
    public class ForfeitCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly ConfigHandler _config;

        public ForfeitCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, ConfigHandler config)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
            _config = config;
        }

        public async Task ForfeitTeam(string team)
        {
            MatchInfo match = (await _database.GetMatchByAttribute(discordChannel: (long) _context.Channel.Id)).First();
            TeamInfo homeTeam = await _database.GetTeamAsync((int) match.TeamHomeID);
            TeamInfo awayTeam = await _database.GetTeamAsync((int) match.TeamAwayID);
            var confirmButton = new ComponentBuilder()
                    .WithButton("Confirm", $"ff_confirm_{_context.Channel.Id}");

            if (team == "home")
            {
                //TODO Check This
                await _context.Interaction.RespondAsync($"{homeTeam.TeamName} Will take an L\n{awayTeam.TeamName} Will take a W", ephemeral: true, components: confirmButton.Build());
                var temp = await _interactivity.NextButtonAsync(u => ((SocketMessageComponent)u).Data.CustomId == $"ff_confirm_{_context.Channel.Id}");
                await temp.Value.DeferAsync();

                homeTeam.Losses += 1;
                awayTeam.Wins += 1;
                await _database.UpdateTeamAsync(homeTeam);
                await _database.UpdateTeamAsync(awayTeam);

                match.HomeTeamWon = false;
                match.MatchFinished = true;
                await _database.UpdateMatchAsync(match);
            }
            else if (team == "away")
            {
                //TODO Check This
                await _context.Interaction.RespondAsync($"{homeTeam.TeamName} Will take an L\n{awayTeam.TeamName} Will take a W", ephemeral: true, components: confirmButton.Build());
                var temp = await _interactivity.NextButtonAsync(u => ((SocketMessageComponent)u).Data.CustomId == $"ff_confirm_{_context.Channel.Id}");
                await temp.Value.DeferAsync();
                
                homeTeam.Wins += 1;
                awayTeam.Losses += 1;
                await _database.UpdateTeamAsync(homeTeam);
                await _database.UpdateTeamAsync(awayTeam);

                match.HomeTeamWon = true;
                match.MatchFinished = true;
                await _database.UpdateMatchAsync(match);
            }
            else
            {
                await _context.Interaction.RespondAsync("Error", ephemeral: true);
            }
        }
    }
}
