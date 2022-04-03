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
using Interactivity.Pagination;

namespace RutgersDiscord.Commands.User
{
    public class MatchSchedule
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        public MatchSchedule(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task GetMatchSchedule(bool showChannelIDs)
        {
            IEnumerable<MatchInfo> matches = await _database.GetMatchByAttribute(matchFinished: false);
            matches = matches.OrderBy(x => x.MatchTime);
            List<ScheduleInfo> schedules = new List<ScheduleInfo>();

            foreach (MatchInfo match in matches)
            {
                ScheduleInfo si = new((long) match.MatchTime);
                si.homeTeam = (await _database.GetTeamAsync((int)match.TeamHomeID)).TeamName;
                si.awayTeam = (await _database.GetTeamAsync((int)match.TeamAwayID)).TeamName;
                schedules.Add(si);
            }

            List<PageBuilder> pages = new();
            Dictionary<IEmote, PaginatorAction> emotes = new Dictionary<IEmote, PaginatorAction>();
            var backwardemote = new Emoji("\u25C0\uFE0F");
            var forwardemote = new Emoji("\u25B6\uFE0F");
            emotes.Add(backwardemote, PaginatorAction.Backward); emotes.Add(forwardemote, PaginatorAction.Forward);

            if (matches.Count() % 10 == 0)
            {
                for (int i = 0; i < (matches.Count() / 10); i++)
                {
                    string homeTeams = string.Join("\r\n", schedules.Select(x => x.homeTeam).Take(10));
                    string awayTeams = string.Join("\r\n", schedules.Select(x => x.awayTeam).Take(10));
                    string gameTimes = string.Join("\r\n", schedules.Select(x => x.toTime()).Take(10));
                    pages.Add(new PageBuilder()
                        .WithTitle("Upcoming Matches:")
                        .WithColor(Color.DarkBlue)
                        .WithFooter("Rutgers CS:GO")
                        .AddField("Team 1", homeTeams, true)
                        .AddField("Team 2", awayTeams, true)
                        .AddField("Match Time", gameTimes, true));
                    schedules.RemoveRange(0, Math.Min(10, schedules.Count()));
                }
            }
            else
            {
                for (int i = 0; i <= (matches.Count() / 10); i++)
                {
                    string homeTeams = string.Join("\r\n", schedules.Select(x => x.homeTeam).Take(10));
                    string awayTeams = string.Join("\r\n", schedules.Select(x => x.awayTeam).Take(10));
                    string gameTimes = string.Join("\r\n", schedules.Select(x => x.toTime()).Take(10));
                    pages.Add(new PageBuilder()
                        .WithTitle("Upcoming Matches:")
                        .WithColor(Color.DarkBlue)
                        .WithFooter("Rutgers CS:GO")
                        .AddField("Team 1", homeTeams, true)
                        .AddField("Team 2", awayTeams, true)
                        .AddField("Match Time", gameTimes, true));
                    schedules.RemoveRange(0, Math.Min(10, schedules.Count()));
                }
            }

            var paginator = new StaticPaginatorBuilder()
                .WithUsers(_context.User)
                .WithPages(pages)
                .WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users)
                .WithEmotes(emotes)
                .Build();

            await _context.Interaction.RespondAsync("Retrieving schedule");
            var msg = await _context.Interaction.GetOriginalResponseAsync();
            await _interactivity.SendPaginatorAsync(paginator, _context.Channel, TimeSpan.FromMinutes(2), msg);
            await msg.DeleteAsync();
        }
    }

    class ScheduleInfo
    {
        public string homeTeam { get; set; }
        public string awayTeam { get; set; }
        public long matchTime { get; set; }

        public ScheduleInfo(long _matchTime)
        {
            matchTime = _matchTime;
        }

        public string toTime()
        {
            DateTime discordEpoch = new DateTime(1970, 1, 1);
            double dateSpan = (new DateTime((long)matchTime).ToUniversalTime() - discordEpoch).TotalSeconds;
            return $"<t:{dateSpan}:f>";
        }

        public override string ToString()
        {
            DateTime discordEpoch = new DateTime(1970, 1, 1);
            double dateSpan = (new DateTime((long)matchTime).ToUniversalTime() - discordEpoch).TotalSeconds;
            return $"{homeTeam} **VS** {awayTeam} **@** <t:{dateSpan}:f>";
        }
    }
}
