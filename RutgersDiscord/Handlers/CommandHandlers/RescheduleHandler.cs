using Discord;
using Discord.WebSocket;
using FluentScheduler;
using Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Handlers.CommandHandlers
{
    class RescheduleHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;
        private readonly ConfigHandler _config;
        private readonly ScheduleHandler _schedule;

        public RescheduleHandler(DiscordSocketClient client, DatabaseHandler database, InteractivityService interactivity, ConfigHandler config, ScheduleHandler schedule)
        {
            _client = client;
            _database = database;
            _interactivity = interactivity;
            _config = config;
            _schedule = schedule;
        }

        public void SubscribeHandlers()
        {
            _client.ButtonExecuted += RescheduleButtonHandler;
        }

        public struct Data
        {
            public string commandName;
            public string response;
            public DateTime timeRequested;
            public ulong requiredUser;
            public ulong originalUser;

            public Data(string[] data)
            {
                commandName = data[0];
                response = data[1];
                timeRequested = new DateTime(long.Parse(data[2]));
                requiredUser = ulong.Parse(data[3]);
                originalUser = ulong.Parse(data[4]);
            }
        }
        public async Task RescheduleButtonHandler(SocketMessageComponent interaction)
        {
            if (!interaction.Data.CustomId.StartsWith("reschedule"))
            {
                return;
            }

            //Data format
            //Command name, matchID, Response(Accept, Reject, Rescind), Time requested, Required User
            Data data = new(interaction.Data.CustomId.Split("_"));

            IEnumerable<MatchInfo> matches = (await _database.GetMatchByAttribute(discordChannel: (long?)interaction.Channel.Id)).OrderBy(m => m.SeriesID);
            MatchInfo match = matches.FirstOrDefault();
            if (match == null)
            {
                await interaction.RespondAsync("Match not found", ephemeral: true);
                return;
            }

            if(data.requiredUser != interaction.User.Id)
            {
                await interaction.RespondAsync("User cannot respond to this interaction", ephemeral: true);
                return;
            }

            if (DateTime.Now - interaction.Message.CreatedAt.DateTime > TimeSpan.FromDays(1))
            {
                await interaction.ModifyOriginalResponseAsync(m => { m.Components = null;m.Embed.Value.ToEmbedBuilder().WithColor(Constants.EmbedColors.reject).Build(); }) ;
                await interaction.RespondAsync("Interaction was expired", ephemeral: true);
                return;
            }

            EmbedBuilder originalEmbed = interaction.Message.Embeds.First().ToEmbedBuilder();

            if (data.response == "reject")
            {
                await interaction.Message.ModifyAsync(m => { m.Components = null; m.Embed = originalEmbed.WithColor(Constants.EmbedColors.reject).Build(); });
                EmbedBuilder embedFollowup = new EmbedBuilder()
                    .WithColor(Constants.EmbedColors.reject)
                    .WithTitle("Reschedule Rejected");
                await interaction.RespondAsync($"<@{data.originalUser}>", embed: embedFollowup.Build());

                //Log
                await _config.LogAsync("Reschedule Handler", $"Status: `Rejected`\n Original User: <@{data.originalUser}>", interaction.User.Id, interaction.Channel.Id);
            }
            else if (data.response == "accept")
            {
                TeamInfo team1 = await _database.GetTeamAsync((int)match.TeamHomeID);
                TeamInfo team2 = await _database.GetTeamAsync((int)match.TeamAwayID);

                await interaction.Message.ModifyAsync(m => { m.Components = null; m.Embed = originalEmbed.WithColor(Constants.EmbedColors.accept).Build(); });
                foreach(MatchInfo m in matches)
                {
                    m.MatchTime = data.timeRequested.Ticks;
                    await _database.UpdateMatchAsync(m);
                }

                DateTime discordEpoch = new DateTime(1970, 1, 1);
                double dateSpan = (data.timeRequested.ToUniversalTime() - discordEpoch).TotalSeconds;
                EmbedBuilder embedFollowup = new EmbedBuilder()
                    .WithColor(Constants.EmbedColors.accept)
                    .WithTitle("Reschedule Accepted")
                    .WithDescription($"New match time\n" +
                                     $"<t:{dateSpan}:f>");
                await interaction.Channel.SendMessageAsync($"<@{team1.Player1}> <@{team1.Player2}> <@{team2.Player1}> <@{team2.Player2}>", embed: embedFollowup.Build());

                //Remove Previous Jobs
                if (JobManager.GetSchedule($"[match_15m_{match.MatchID}]") != null)
                {
                    JobManager.RemoveJob($"[match_15m_{match.MatchID}]");

                    //Log
                    await _config.LogAsync("JobManager", $"Status: `Job Removed` \nJob: `[match_15m_{match.MatchID}]`");
                }
                if (JobManager.GetSchedule($"[match_24h_{match.MatchID}]") != null)
                {
                    JobManager.RemoveJob($"[match_24h_{match.MatchID}]");

                    //Log
                    await _config.LogAsync("JobManager", $"Status: `Job Removed` \nJob: `[match_15m_{match.MatchID}]`");
                }

                //Add new Jobs
                List<long> players = new() { team1.Player1, team1.Player2, team2.Player1, team2.Player2 };
                if (data.timeRequested > DateTime.Now.AddMinutes(15))
                {
                    JobManager.AddJob(async () => await _schedule.MentionUsers((ulong)match.DiscordChannel, players, false), s => s.WithName($"[match_15m_{match.MatchID}]").ToRunOnceAt(new DateTime((long)match.MatchTime) - TimeSpan.FromMinutes(15)));

                    //Log
                    await _config.LogAsync("JobManager", $"Status: `Job Added` \nJob: `[match_15m_{match.MatchID}]`");
                }
                if (data.timeRequested > DateTime.Now.AddDays(1))
                {
                    JobManager.AddJob(async () => await _schedule.MentionUsers((ulong)match.DiscordChannel, players, true), s => s.WithName($"[match_24h_{match.MatchID}]").ToRunOnceAt(new DateTime((long)match.MatchTime) - TimeSpan.FromDays(1)));

                    //Log
                    await _config.LogAsync("JobManager", $"Status: `Job Added` \nJob: `[match_24h_{match.MatchID}]`");
                }

                //Log
                await _config.LogAsync("Reschedule Handler", $"Status: `Success`\nOriginal User: <@{data.originalUser}>", interaction.User.Id, interaction.Channel.Id);
            }
            else if (data.response == "rescind")
            {
                await interaction.Message.ModifyAsync(m => { m.Components = null; m.Embed = originalEmbed.WithColor(Constants.EmbedColors.reject).Build(); });
                EmbedBuilder embedFollowup = new EmbedBuilder()
                    .WithColor(Constants.EmbedColors.reject)
                    .WithTitle("Reschedule Rescinded");
                await interaction.RespondAsync(embed: embedFollowup.Build());

                //Log
                await _config.LogAsync("Reschedule Handler", "Status: `Cancelled`", interaction.User.Id, interaction.Channel.Id);
            }
        }
    }
}
