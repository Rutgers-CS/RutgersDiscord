﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FluentScheduler;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RutgersDiscord.Commands.User
{
    public class RescheduleCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        public RescheduleCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task RescheduleMatch(DateTime date)
        {

            //TODO move these checks to some sort of precondition check
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

            if (date < DateTime.Now)
            {
                await _context.Interaction.RespondAsync("Match cannot be rescheduled for past time", ephemeral: true);
                return;
            }

            DateTime discordEpoch = new DateTime(1970, 1, 1);
            double originalDateSpan = (new DateTime((long)match.MatchTime) - discordEpoch).TotalSeconds;
            double dateSpan = (date.ToUniversalTime() - discordEpoch).TotalSeconds;
            TeamInfo teamOpponent;
            if (match.TeamHomeID == team.TeamID)
            {
                teamOpponent = await _database.GetTeamAsync((int)match.TeamAwayID);
            }
            else
            {
                teamOpponent = await _database.GetTeamAsync((int)match.TeamHomeID);
            }

            if (match.TeamHomeReady == true)
            {
                await _context.Interaction.RespondAsync("Please unready before rescheduling match", ephemeral: true);
                return;
            }
            Random r = new();
            int commandID = r.Next();
            ComponentBuilder component = new ComponentBuilder()
                .WithButton("Accept Reschedule", $"reschedule_{match.MatchID}_{commandID}_accept")
                .WithButton("Reject Reschedule", $"reschedule_{match.MatchID}_{commandID}_reject");
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Constants.EmbedColors.active)
                .WithTitle($"{_context.User.Username} requested a reschedule")
                .AddField("Original Time", $"<t:{originalDateSpan}:f>")
                .AddField("Proposed Time", $"<t:{dateSpan}:f>");

            await _context.Interaction.RespondAsync($"<@{teamOpponent.Player1}>", embed: embed.Build(), components: component.Build());

            var response = await _interactivity.NextInteractionAsync(
                s => (s.User.Id == (ulong)teamOpponent.Player1
                && ((SocketMessageComponent)s).Data.CustomId.StartsWith($"reschedule_{match.MatchID}_{commandID}")),
                timeout:TimeSpan.FromHours(1));

            ComponentBuilder componentEmpty = new ComponentBuilder();
            await _context.Interaction.ModifyOriginalResponseAsync(m => m.Components = componentEmpty.Build());

            if (response.IsTimeouted)
            {
                await _context.Interaction.ModifyOriginalResponseAsync(m => m.Embed = embed.WithColor(Constants.EmbedColors.reject).Build());
                await _context.Channel.SendMessageAsync("Request timed out");
                return;
            }

            if (((SocketMessageComponent)response.Value).Data.CustomId == $"reschedule_{match.MatchID}_{commandID}_reject")
            {
                await _context.Interaction.ModifyOriginalResponseAsync(m => m.Embed = embed.WithColor(Constants.EmbedColors.reject).Build());
                return;
            }

            if(((SocketMessageComponent)response.Value).Data.CustomId == $"reschedule_{match.MatchID}_{commandID}_accept")
            {
                await _context.Interaction.ModifyOriginalResponseAsync(m => m.Embed = embed.WithColor(Constants.EmbedColors.accept).Build());
                match.MatchTime = date.Ticks;
                await _database.UpdateMatchAsync(match);
                JobManager.GetSchedule($"[match_{match.MatchID}]").ToRunOnceAt(date);
                return;
            }
            //Code shouldn't reach here
        }
    }
}