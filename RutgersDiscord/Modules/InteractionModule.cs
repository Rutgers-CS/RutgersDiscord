using CoreRCON;
using Discord;
using Interactivity;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RutgersDiscord.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractivityService _interactive;
        private readonly IServiceProvider _services;

        public InteractionModule(DiscordSocketClient client, InteractivityService interactive, IServiceProvider services)
        {
            _client = client;
            _interactive = interactive;
            _services = services;
        }

        [SlashCommand("echo", "Echo an input", runMode: RunMode.Async)]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }

        //Not sure the correct placement/format for all of this, feel free to move it
        [SlashCommand("register", "Provide required information to register for the event")]
        public async Task Register()
        {
            var modalBuilder = new ModalBuilder()
                .WithTitle("Event Registration")
                .WithCustomId("registration_form")
                .AddTextInput("Your Name:", "player_name", required:true)
                .AddTextInput("Link to Steam profile:", "steam_url", required:true)
                .AddTextInput("Your team name:", "team_name", required:true)
                ;

            await Context.Interaction.RespondWithModalAsync(modalBuilder.Build());

            _client.ModalSubmitted += async modal =>
            {
                List<SocketMessageComponentData> components = modal.Data.Components.ToList();

                //TODO: Replace with database connection
                string playerName = components.First(x => x.CustomId == "player_name").Value;
                string steamURL = components.First(x => x.CustomId == "steam_url").Value;
                string teamName = components.First(x => x.CustomId == "team_name").Value;

                string steamID = ResolveURL(steamURL).Result;

                string msg = $"{playerName} with {steamID} on {teamName}";

                await modal.RespondAsync(msg);
            };
        }

        public async Task<string> ResolveURL(string url)
        {
            string trimmedURL = url.Replace("steamcommunity.com/id/", "").Replace("https://", "").Replace("/","");
            long steamID64;

            bool vanityURL = true;
            if (trimmedURL.All(char.IsDigit)) vanityURL = false;

            if (vanityURL)
            {
                string requestUrl = $"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={Environment.GetEnvironmentVariable("steamWebAPIToken")}&vanityurl=" + trimmedURL;
                HttpClient steamAPIClient = new HttpClient();
                HttpResponseMessage response = await steamAPIClient.GetAsync(requestUrl);
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic deserializedResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

                if(deserializedResponse.response.success == "1")
                {
                    string steamid = deserializedResponse.response.steamid;
                    steamID64 = long.Parse(steamid);
                }
                else
                {
                    return "error";
                }
            }
            else
            {
                steamID64 = long.Parse(trimmedURL);
            }

            var universe = (steamID64 >> 56) & 0xFF;
            if (universe == 1) universe = 0;

            var accountIDLowBit = steamID64 & 1;
            var accountIDHighBit = (steamID64 >> 1) & 0x7FFFFFF;

            string steamID = "STEAM_" + universe + ":" + accountIDLowBit + ":" + accountIDHighBit;
            return steamID;
        }

        [SlashCommand("ready", "Set your team as ready for the match")]
        public async Task TeamReady()
        {
            //Replace with database lookup of team name based on discord id
            string teamName = "testing";
            await RespondAsync($"{teamName} is ready for the match");
            //Tell MatchHandler that the team is ready
        }

        [SlashCommand("unready", "Set your team as not ready for the match")]
        public async Task TeamUnReady()
        {
            //Replace with database lookup of team name based on discord id
            string teamName = "testing";
            await RespondAsync($"{teamName} is not ready for the match");
            //Tell MatchHandler that the team is not ready
        }

        [SlashCommand("admin", "Notify an admin")]
        public async Task NotifyAdmin()
        {
            //Ping @guihori, @0p7day, @galifi in eboard or smth
        }

        [SlashCommand("leaderboard", "Display the tournament leaderboard")]
        public async Task DisplayLeaderboard()
        {
            //TODO
        }

        [SlashCommand("teamselction", "Select or create a team")]
        public async Task TeamSelection()
        {
            var builder = new ComponentBuilder()
                .WithButton("Create a new team", "new_team")
                .WithButton("Join existing team", "join_team")
                .WithButton("Looking for team", "no_team")
                ;

            await ReplyAsync("test", components: builder.Build());
            _client.ButtonExecuted += TeamButtonHandler;
        }

        public async Task TeamButtonHandler(SocketMessageComponent component)
        {
            List<string> current_teams = new List<string>();
            current_teams.Add("test1");
            current_teams.Add("test2");

            var newTeamModal = new ModalBuilder()
                .WithTitle("New Team")
                .WithCustomId("new_team_modal")
                .AddTextInput("Team Name", "new_team_name");

            var existingTeamSelect = new SelectMenuBuilder()
                .WithPlaceholder("Select a team")
                .WithCustomId("existing_team_selection")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (string team in current_teams)
            {
                existingTeamSelect.AddOption(team.ToString(), team.ToString());
            }

            var selectBuilder = new ComponentBuilder()
                .WithSelectMenu(existingTeamSelect);

            switch(component.Data.CustomId)
            {
                case "new_team":
                    //Create new team
                    await component.RespondWithModalAsync(newTeamModal.Build());
                    break;
                case "join_team":
                    //Pick existing team
                    await component.RespondAsync("Select a team", components: selectBuilder.Build());
                    break;
                case "no_team":
                    //Flag as looking for team
                    await component.RespondAsync("No team");
                    break;
            }
        }
    }
}