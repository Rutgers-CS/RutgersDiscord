using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Interactions;
using Discord;
using System.Text;
using System.Threading.Tasks;
using Interactivity;
using RutgersDiscord.Handlers;
using Discord;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;


namespace RutgersDiscord.Commands.User
{
    public class RegisterCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly RegistrationHandler _registrationHandler;

        public RegisterCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, RegistrationHandler registrationHandler)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
            _registrationHandler = registrationHandler;
        }

        public async Task RegistrationForm()
        {
            var modalBuilder = new ModalBuilder()
                .WithTitle("Event Registration")
                .WithCustomId("register_form")
                .AddTextInput("Your Name:", "player_name", required: true, placeholder: "Gabe Newell", style: TextInputStyle.Short, minLength: 3, maxLength: 40)
                .AddTextInput("Link to Steam profile:", "steam_url", required: true, placeholder: "https://steamcommunity.com/id/gabelogannewell", style: TextInputStyle.Short, minLength: 22)
                ;

            await _context.Interaction.RespondWithModalAsync(modalBuilder.Build());
        }
    }
}
