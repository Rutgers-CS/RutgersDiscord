using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RutgersDiscord.Commands.User
{
    public class NotifyAdminCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;



        public NotifyAdminCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task CallAdmin()
        {
            ulong discid = 860410058961059890; //change to private admin channel
            ulong adminroleid = 670685962908598273; //change to admin role (mafioso)
            var chnl = _client.GetChannel(discid) as IMessageChannel;
            await chnl.SendMessageAsync("**Admin required** " + $"<@&{adminroleid}>" + "\n" + "Requested by: " + _context.User.Mention + $" in <#{_context.Channel.Id}>.");
            await _context.Interaction.RespondAsync("Admins have been notified.");

        }


    }

}