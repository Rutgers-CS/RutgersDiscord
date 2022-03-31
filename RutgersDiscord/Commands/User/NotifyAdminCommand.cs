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
    public class NotifyAdminCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly ConfigHandler _config;



        public NotifyAdminCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, ConfigHandler config)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
            _config = config;
        }

        public async Task CallAdmin()
        {
            ulong discid = _config.settings.DiscordSettings.Channels.SCAdmin;
            ulong adminroleid = _config.settings.DiscordSettings.Roles.Admin;
            var chnl = _client.GetChannel(discid) as IMessageChannel;
            /*var match = (await _database.GetMatchByAttribute(discordChannel: (long?)chnl.Id)).FirstOrDefault();
            match.AdminCalled = true;
            await _database.UpdateMatchAsync(match)*/;
            await chnl.SendMessageAsync("**Admin required** " + $"<@&{adminroleid}>" + "\n" + "Requested by: " + _context.User.Mention + $" in <#{_context.Channel.Id}>.");
            await _context.Interaction.RespondAsync("Admins have been notified.");
            //TODO add button to resolve instead of command
        }

    }

}