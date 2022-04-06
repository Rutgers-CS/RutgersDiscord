using Discord;
using Discord.Interactions;
using Discord.Rest;
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
        private readonly ScheduleHandler _schedule;
        private readonly ConfigHandler _config;

        public RescheduleCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, ScheduleHandler schedule, ConfigHandler config)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
            _schedule = schedule;
            _config = config;
        }

        public async Task RescheduleMatch(DateTime date)
        {

            //TODO move these checks to some sort of precondition check
            MatchInfo match = (await _database.GetMatchByAttribute(discordChannel: (long?)_context.Channel.Id)).FirstOrDefault();
            if (match == null)
            {
                await _config.LogAsync("Reschedule Command", "Status: `failed` \nReason: `match not found`",_context.User.Id,_context.Channel.Id);
                await _context.Interaction.RespondAsync("Match not found", ephemeral: true);
                return;
            }

            TeamInfo team = await _database.GetTeamByDiscordIDAsync((long)_context.Interaction.User.Id, true);
            if (team == null)
            {
                //Log
                await _config.LogAsync("Reschedule Command", "Status: `failed` \nReason: `user not found`", _context.User.Id, _context.Channel.Id);
                await _context.Interaction.RespondAsync("User not captain of a team", ephemeral: true);
                return;
            }

            if (date < DateTime.Now)
            {
                //Log
                await _config.LogAsync("Reschedule Command", "Status: `failed` \nReason: `wrong time input`", _context.User.Id, _context.Channel.Id);
                await _context.Interaction.RespondAsync("Match cannot be rescheduled for past time", ephemeral: true);
                return;
            }

            //Log
            await _config.LogAsync("Reschedule Command", "Status: `In progress` \nDescription: `awaiting opponent response`", _context.User.Id, _context.Channel.Id);

            DateTime discordEpoch = new DateTime(1970, 1, 1);
            double originalDateSpan = (new DateTime((long)match.MatchTime).ToUniversalTime() - discordEpoch).TotalSeconds;
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
                .WithButton("Accept Reschedule", $"reschedule_accept_{date.Ticks}_{teamOpponent.Player1}_{_context.User.Id}",ButtonStyle.Primary)
                .WithButton("Reject Reschedule", $"reschedule_reject_{date.Ticks}_{teamOpponent.Player1}_{_context.User.Id}",ButtonStyle.Danger)
                .WithButton("Rescind", $"reschedule_rescind_{date.Ticks}_{_context.User.Id}_{_context.User.Id}",ButtonStyle.Secondary);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Constants.EmbedColors.active)
                .WithTitle($"{_context.User.Username} requested a reschedule")
                .AddField("Original Time", $"<t:{originalDateSpan}:f>")
                .AddField("Proposed Time", $"<t:{dateSpan}:f>");

            await _context.Interaction.RespondAsync($"<@{teamOpponent.Player1}>", embed: embed.Build(), components: component.Build());
            RestUserMessage reply = (await _context.Interaction.GetOriginalResponseAsync()) as RestUserMessage;


            //Wait 1 day then time out request
            System.Threading.Thread.Sleep(86400000); //86400000
            await reply.UpdateAsync();
            var replyEmbed = reply.Embeds.First().ToEmbedBuilder();
            if(replyEmbed.Color == Constants.EmbedColors.active)
            {
                await reply.ModifyAsync(m => { m.Components = null; m.Embed = replyEmbed.WithColor(Constants.EmbedColors.reject).Build(); });
                EmbedBuilder embedFollowup = new EmbedBuilder()
                    .WithColor(Constants.EmbedColors.reject)
                    .WithTitle("Reschedule timed out");
                await reply.Channel.SendMessageAsync($"<@{team.Player1}>", embed: embedFollowup.Build());

                //Log
                await _config.LogAsync("Reschedule Command", $"Status: `failed`\nReason: `timed out`", _context.User.Id, _context.Channel.Id);
            }
        }
    }
}