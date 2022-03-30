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
        //Cancel Request
        //Send Response
        //Check if match exists
        //Fix match room permission

        public async Task ForfeitTeam(string team)
        {
            MatchInfo match = (await _database.GetMatchByAttribute(discordChannel: (long)_context.Channel.Id)).FirstOrDefault();
            if (match != null)
            {
                TeamInfo homeTeam = await _database.GetTeamAsync((int)match.TeamHomeID);
                TeamInfo awayTeam = await _database.GetTeamAsync((int)match.TeamAwayID);
                var confirmButton = new ComponentBuilder()
                        .WithButton("Confirm", $"ff_confirm_{_context.Channel.Id}")
                        .WithButton("Cancel", $"ff_cancel_{_context.Channel.Id}");

                if (team == "home")
                {
                    await _context.Interaction.RespondAsync($"{homeTeam.TeamName} Will take an L\n{awayTeam.TeamName} Will take a W", ephemeral: true, components: confirmButton.Build());
                    var temp = await _interactivity.NextButtonAsync(u => ((SocketMessageComponent)u).Data.CustomId == $"ff_confirm_{_context.Channel.Id}" || ((SocketMessageComponent)u).Data.CustomId == $"ff_cancel_{_context.Channel.Id}");
                    await temp.Value.DeferAsync();
                    if (((SocketMessageComponent)temp.Value).Data.CustomId.StartsWith("ff_confirm"))
                    {
                        homeTeam.Losses += 1;
                        awayTeam.Wins += 1;
                        await _database.UpdateTeamAsync(homeTeam);
                        await _database.UpdateTeamAsync(awayTeam);

                        match.HomeTeamWon = false;
                        match.MatchFinished = true;
                        await _database.UpdateMatchAsync(match);

                        //Stupid way to remove perms
                        SocketGuild guild = _client.GetGuild(_config.settings.DiscordSettings.Guild);
                        SocketTextChannel channel = guild.GetTextChannel((ulong) _context.Channel.Id);
                        await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)homeTeam.Player1), new OverwritePermissions(sendMessages: PermValue.Deny));
                        await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)homeTeam.Player2), new OverwritePermissions(sendMessages: PermValue.Deny));
                        await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)awayTeam.Player1), new OverwritePermissions(sendMessages: PermValue.Deny));
                        await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)awayTeam.Player2), new OverwritePermissions(sendMessages: PermValue.Deny));
                        await temp.Value.FollowupAsync($"{homeTeam.TeamName} has forfeited");
                    }
                    else
                    {
                        await temp.Value.FollowupAsync("Forfeit cancelled", ephemeral: true);
                    }
                }
                else if (team == "away")
                {
                    await _context.Interaction.RespondAsync($"{awayTeam.TeamName} Will take an L\n{homeTeam.TeamName} Will take a W", ephemeral: true, components: confirmButton.Build());
                    var temp = await _interactivity.NextButtonAsync(u => ((SocketMessageComponent)u).Data.CustomId == $"ff_confirm_{_context.Channel.Id}" || ((SocketMessageComponent)u).Data.CustomId == $"ff_cancel_{_context.Channel.Id}");
                    await temp.Value.DeferAsync();
                    if (((SocketMessageComponent)temp.Value).Data.CustomId.StartsWith("ff_confirm"))
                    {
                        awayTeam.Losses += 1;
                        homeTeam.Wins += 1;
                        await _database.UpdateTeamAsync(homeTeam);
                        await _database.UpdateTeamAsync(awayTeam);

                        match.HomeTeamWon = false;
                        match.MatchFinished = true;
                        await _database.UpdateMatchAsync(match);

                        //Stupid way to remove perms
                        SocketGuild guild = _client.GetGuild(_config.settings.DiscordSettings.Guild);
                        SocketTextChannel channel = guild.GetTextChannel((ulong)_context.Channel.Id);
                        await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)homeTeam.Player1), new OverwritePermissions(sendMessages: PermValue.Deny));
                        await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)homeTeam.Player2), new OverwritePermissions(sendMessages: PermValue.Deny));
                        await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)awayTeam.Player1), new OverwritePermissions(sendMessages: PermValue.Deny));
                        await channel.AddPermissionOverwriteAsync(guild.GetUser((ulong)awayTeam.Player2), new OverwritePermissions(sendMessages: PermValue.Deny));

                        await temp.Value.FollowupAsync($"{awayTeam.TeamName} has forfeited");
                    }
                    else
                    {
                        await temp.Value.FollowupAsync("Forfeit cancelled", ephemeral: true);
                    }
                }
                else
                {
                    await _context.Interaction.RespondAsync("I can't believe you've done this", ephemeral: true);
                }
            }
            else
            {
                await _context.Interaction.RespondAsync("Can't find match", ephemeral: true);
            }
        }
    }
}
