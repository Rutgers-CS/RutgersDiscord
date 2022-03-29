using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Interactivity.Pagination;


namespace RutgersDiscord.Commands.User
{

    public class LeaderboardTeamData
    {
        public string Name { get; set; }
        public int Win { get; set; }
        public int Loss { get; set; }
        public int WLDiff { get; set; }
        public int RoundWin { get; set; }
        public int RoundLoss { get; set; }
        public int RoundDiff { get; set; }
        public float KD { get; set; }
        public string FMap { get; set; }

    }

    public static class IsNullOrEmptyExtension
    {
        public static bool IsNullOrEmpty(this IEnumerable source)
        {
            if (source != null)
            {
                foreach (object obj in source)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source != null)
            {
                foreach (T obj in source)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class LeaderboardCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        List<LeaderboardTeamData> teamsdata = new();

        private List<LeaderboardTeamData> SortBy(List<LeaderboardTeamData> teamsdata, string input)
        {
            if(input == "record")
            {
                return teamsdata.OrderByDescending(x => x.Win).ThenByDescending(x => x.WLDiff).ThenByDescending(x => x.RoundDiff).ToList();
            } 
            else if (input == "kd") 
            {
                return teamsdata.OrderByDescending(x => x.KD).ThenByDescending(x => x.Win).ThenByDescending(x => x.WLDiff).ToList();
            }
            else if (input == "rd")
            {
                return teamsdata.OrderByDescending(x => x.RoundDiff).ThenByDescending(x => x.Win).ThenByDescending(x => x.WLDiff).ToList();
            }
            else 
            {
                return teamsdata.OrderByDescending(x => x.Win).ThenByDescending(x => x.WLDiff).ThenByDescending(x => x.RoundDiff).ToList();
            }
        }

        public LeaderboardCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task PullLeaderboard(string sortBy)
        {

            IEnumerable<TeamInfo> datateams = await _database.GetAllTeamsAsync();
            foreach (var team in datateams)
            {
                LeaderboardTeamData teamdata = new LeaderboardTeamData();
                teamdata.Name = team.TeamName;

                var winc = team.Wins;
                var losc = team.Losses;
                if (team.Wins == null)
                    winc = 0;
                if (team.Losses == null)
                    losc = 0;

                teamdata.Win = (int)winc;
                teamdata.Loss = (int)losc;
                teamdata.WLDiff = (int)winc - (int)losc;


                var rwinc = team.RoundWins;
                var rlosc = team.RoundLosses;
                if (team.RoundWins == null)
                    rwinc = 0;
                if (team.RoundLosses == null)
                    rlosc = 0;

                teamdata.RoundWin = (int)rwinc;
                teamdata.RoundLoss = (int)rlosc;
                teamdata.RoundDiff = (int)rwinc - (int)rlosc;

                float p1k;
                float p2k;
                float p1d;
                float p2d;
                if (team.Player1 == 0)
                {
                    p1k = 0f;
                    p1d = 1f;

                }
                else
                {
                    var player1 = await _database.GetPlayerAsync(team.Player1);
                    if (player1.Kills.HasValue)
                    {
                        p1k = (float)player1.Kills;

                    }
                    else
                    {
                        p1k = 0f;
                    }

                    if (player1.Deaths.HasValue)
                    {
                        if (player1.Deaths == 0)
                        {
                            p1d = 1f;
                        }
                        else
                        {
                            p1d = (float)player1.Deaths;
                        }

                    }
                    else
                    {
                        p1d = 1f;
                    }

                }
                if (team.Player2 == 0)
                {
                    p2k = 0f;
                    p2d = 1f;

                }
                else
                {
                    var player2 = await _database.GetPlayerAsync(team.Player2);
                    if (player2.Kills.HasValue)
                    {
                        p2k = (float)player2.Kills;

                    }
                    else
                    {
                        p2k = 0f;
                    }

                    if (player2.Deaths.HasValue)
                    {
                        if (player2.Deaths == 0)
                        {
                            p2d = 1f;
                        }
                        else
                        {
                            p2d = (float)player2.Deaths;
                        }

                    }
                    else
                    {
                        p2d = 1f;
                    }
                }
                float kd = (p1k / p1d + p2k / p2d) / 2f;

                teamdata.KD = kd;

                IEnumerable<MatchInfo> teamsmatches = await _database.GetMatchByAttribute(teamID1: team.TeamID);
                if (teamsmatches.IsNullOrEmpty())
                {
                    teamdata.FMap = "None";
                }
                else
                {

                    teamsmatches = teamsmatches.Where(i => i.MapID != null);

                    if (!teamsmatches.Any())
                    {
                        teamdata.FMap = "None";
                    }
                    else
                    {
                        var most = (from k in teamsmatches

                                    group k.MapID by k.MapID into grp
                                    orderby grp.Count() descending
                                    select grp.Key).First();

                        teamdata.FMap = (await _database.GetMapAsync((int)most)).MapName;

                    }
                }
                teamsdata.Add(teamdata);
            }

            teamsdata = SortBy(teamsdata, sortBy);

            List<string> teams = teamsdata.Select(c => c.Name).ToList();
            List<string> records = teamsdata.Select(c => c.Win.ToString() + "-" + c.Loss.ToString()).ToList();
            List<string> diffs = teamsdata.Select(c => { 

            if (c.RoundDiff > 0)
            {
                return c.RoundDiff.ToString("+0");
            }
            else if (c.RoundDiff < 0)
            {
                return c.RoundDiff.ToString();
            } 
            else
            {
                return c.RoundDiff.ToString("±0");
            }
            }
            ).ToList();

            List<string> kds = teamsdata.Select(c => c.KD.ToString("0.00")).ToList();
            List<string> fmaps = teamsdata.Select(c => c.FMap.ToString()).ToList();

            List<PageBuilder> pages = new();
            Dictionary<IEmote, PaginatorAction> emotes = new Dictionary<IEmote, PaginatorAction>();

            var backwardemote = new Emoji("\u25C0\uFE0F"); var forwardemote = new Emoji("\u25B6\uFE0F");
            emotes.Add(backwardemote, PaginatorAction.Backward); emotes.Add(forwardemote, PaginatorAction.Forward);
          
            var originalsize = teamsdata.Count;
            if (originalsize % 5 == 0)
            {
                for (int i = 0; i < (originalsize / 5); i++)
                { 
                    string l = GenerateLeaderboardPage(teams.Take(5).ToList(), records.Take(5).ToList(), diffs.Take(5).ToList(), kds.Take(5).ToList(), fmaps.Take(5).ToList());
                    pages.Add(new PageBuilder().WithTitle("Scarlet Classic's Leaderboard").WithDescription($"```{l}```").WithColor(new Color(102, 0, 0)).WithFooter("Rutgers CS:GO"));
                    teams.RemoveRange(0, Math.Min(5, teams.Count)); records.RemoveRange(0, Math.Min(5, records.Count)); diffs.RemoveRange(0, Math.Min(5, diffs.Count)); kds.RemoveRange(0, Math.Min(5, kds.Count)); fmaps.RemoveRange(0, Math.Min(5, fmaps.Count));
                }
            }
            else
            {
                for (int i = 0; i <= (originalsize / 5); i++)
                {
                    string l = GenerateLeaderboardPage(teams.Take(5).ToList(), records.Take(5).ToList(), diffs.Take(5).ToList(), kds.Take(5).ToList(), fmaps.Take(5).ToList());
                    pages.Add(new PageBuilder().WithTitle("Scarlet Classic's Leaderboard").WithDescription($"```{l}```").WithColor(new Color(102, 0, 0)).WithFooter("Rutgers CS:GO"));
                    teams.RemoveRange(0, Math.Min(5, teams.Count)); records.RemoveRange(0, Math.Min(5, records.Count)); diffs.RemoveRange(0, Math.Min(5, diffs.Count)); kds.RemoveRange(0, Math.Min(5, kds.Count)); fmaps.RemoveRange(0, Math.Min(5, fmaps.Count));
                }
            }

            var paginator = new StaticPaginatorBuilder().WithUsers(_context.User).WithPages(pages).WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users).WithEmotes(emotes).Build();

            await _context.Interaction.RespondAsync("retrieving leaderboard");
            var message = await _context.Interaction.GetOriginalResponseAsync();
            await _interactivity.SendPaginatorAsync(paginator, _context.Channel, TimeSpan.FromMinutes(2), message);
            await message.DeleteAsync();

        }

        public static string GenerateLeaderboardPage(List<string> teamlist, List<string> recordlist, List<string> difflist, List<string> kdlist, List<string> maplist)
        {

            int teamcolw = teamlist.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
            int recordcolw = recordlist.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
            int diffcolw = difflist.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
            int kdcolw = kdlist.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
            int mapcolw = maplist.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;

            string CalculateSpace(int colw, int inputL)
            {
                string space = new string(' ', colw - inputL);
                return space;

            }

            string CalculateBorder(int colw, bool thicc)
            {
                string lining;

                if (thicc)
                    lining = new string('═', colw + 2);
                else
                    lining = new string('─', colw + 2);

                return lining;

            }

            string leaderboard = "╔" + CalculateBorder(teamcolw, true) + "╤" + CalculateBorder(recordcolw, true) + "╤" + CalculateBorder(diffcolw, true) + "╤" + CalculateBorder(kdcolw, true) + "╤" + CalculateBorder(mapcolw, true) + "╗" + "\r\n";
            //Console.WriteLine(teamcolw + " " + recordcolw + " " + diffcolw + " " + kdcolw + " " + mapcolw + " ");
            leaderboard += "║ " + "Team" + CalculateSpace(teamcolw, 4) + " │ " + "W/L" + CalculateSpace(recordcolw, 3) + " | " + "RD" + CalculateSpace(diffcolw, 2) + " | " + "K/D" + CalculateSpace(kdcolw, 3) + " | " + "FMap" + CalculateSpace(mapcolw, 4) + " ║" + "\r\n";  //integers are length of header titles
            leaderboard += "╠" + CalculateBorder(teamcolw, true) + "╪" + CalculateBorder(recordcolw, true) + "╪" + CalculateBorder(diffcolw, true) + "╪" + CalculateBorder(kdcolw, true) + "╪" + CalculateBorder(mapcolw, true) + "╣" + "\r\n";
        
            for (int i = 0; i < (teamlist.Count); i++)
            {
                if (teamlist[i].StartsWith("杀")) //mega hardcode
                {
                    leaderboard += "║ " + teamlist[i] + CalculateSpace(teamcolw, teamlist[i].Length * 2 - 1) + " │ " + recordlist[i] + CalculateSpace(recordcolw, recordlist[i].Length) + " | " + difflist[i] + CalculateSpace(diffcolw, difflist[i].Length) + " | " + kdlist[i] + CalculateSpace(kdcolw, kdlist[i].Length) + " | " + maplist[i] + CalculateSpace(mapcolw, maplist[i].Length) + " ║" + "\r\n";
                } 
                else
                {
                    leaderboard += "║ " + teamlist[i] + CalculateSpace(teamcolw, teamlist[i].Length) + " │ " + recordlist[i] + CalculateSpace(recordcolw, recordlist[i].Length) + " | " + difflist[i] + CalculateSpace(diffcolw, difflist[i].Length) + " | " + kdlist[i] + CalculateSpace(kdcolw, kdlist[i].Length) + " | " + maplist[i] + CalculateSpace(mapcolw, maplist[i].Length) + " ║" + "\r\n";
                }

                if (!((i + 1) == teamlist.Count))
                {
                    leaderboard += "╟" + CalculateBorder(teamcolw, false) + "┼" + CalculateBorder(recordcolw, false) + "┼" + CalculateBorder(diffcolw, false) + "┼" + CalculateBorder(kdcolw, false) + "┼" + CalculateBorder(mapcolw, false) + "╢" + "\r\n";
                }
            }

            leaderboard += "╚" + CalculateBorder(teamcolw, true) + "╧" + CalculateBorder(recordcolw, true) + "╧" + CalculateBorder(diffcolw, true) + "╧" + CalculateBorder(kdcolw, true) + "╧" + CalculateBorder(mapcolw, true) + "╝" + "\r\n";
            return leaderboard;

        }
    }
}
