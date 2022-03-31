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
    public class RestartMatchCommand
    {
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly DatHostAPIHandler _datHostAPIHandler;

        public RestartMatchCommand(SocketInteractionContext context, DatabaseHandler database, DatHostAPIHandler datHostAPIHandler)
        {
            _context = context;
            _database = database;
            _datHostAPIHandler = datHostAPIHandler;
        }

        public async Task RestartMap()
        {
            MatchInfo match = (await _database.GetMatchByAttribute(discordChannel: (long)_context.Channel.Id)).First();
            if(match != null)
            {
                string serverID = match.ServerID;
                await _datHostAPIHandler.RestartMatch(serverID);
            }
            else
            {
                await _context.Interaction.RespondAsync("Can't find match", ephemeral: true);
            }
        }
    }
}
