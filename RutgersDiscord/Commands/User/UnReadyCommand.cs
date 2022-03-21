using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Commands.User
{
    public class UnReadyCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        public UnReadyCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task UnReady()
        {
            MatchInfo match = (await _database.GetMatchByAttribute(discordChannel: (long?)_context.Channel.Id)).FirstOrDefault();
            if (match == null)
            {
                await _context.Interaction.RespondAsync("Match not found", ephemeral: true);
                return;
            }

            TeamInfo team = await _database.GetTeamByDiscordIDAsync((long)_context.Interaction.User.Id, true);
            if (team == null)
            {
                await _context.Interaction.RespondAsync("User not captain of a team", ephemeral: true);
                return;
            }

            if(match.TeamHomeReady == true && match.TeamAwayReady == true) // (bool?) moment
            {
                await _context.Interaction.RespondAsync("Cannot unready after match starts", ephemeral: true);
                return;
            }

            //remove ready
            if(team.TeamID == match.TeamHomeID)
            {
                match.TeamHomeReady = false;
            }
            if(team.TeamID == match.TeamAwayID)
            {
                match.TeamAwayReady = false;
            }
            await _database.UpdateMatchAsync(match);

            await _context.Interaction.RespondAsync($"{team.TeamName} is not ready");
        }
    }
}
