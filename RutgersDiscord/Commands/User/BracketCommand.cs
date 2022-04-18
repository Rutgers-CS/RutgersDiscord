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

namespace RutgersDiscord.Commands.User
{
    public class BracketCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly ConfigHandler _config;



        public BracketCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, ConfigHandler config)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
            _config = config;
        }

        public async Task GetBracket()
        {
            var bracket = ("https://challonge.com/sccsgo");
            _interactivity.DelayedSendMessageAndDeleteAsync(_context.Channel, deleteDelay: TimeSpan.FromMinutes(5), text: bracket);
        }

    }

}