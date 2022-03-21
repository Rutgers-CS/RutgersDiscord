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
    public class LeaderboardCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        /*   private List<string> teams = new List<string> { "Team Gal", "Team Kenji", "Team August", "Team GtestBot", "Team Bozo" };   //5 items test
           private List<string> records = new List<string> { "3-2", "5-0", "4-1", "0-5", "1-4" };
           private List<string> diffs = new List<string> { "56-45", "62-38", "58-41", "14-60", "19-56" };
           private List<string> kds = new List<string> { "1.12", "1.43", "1.08", "0.61", "0.67" };
           private List<string> fmaps = new List<string> { "Inferno", "Ancient", "Dust2", "Vertigo", "Overpass" };*/

        private List<string> teams = new List<string> { "Team Gal", "Team Kenji", "Team August", "Team GtestBot", "Team Bozo", "Team notGuihori" };   //6 items test
        private List<string> records = new List<string> { "3-2", "5-0", "4-1", "0-5", "1-4", "55-0" };
        private List<string> diffs = new List<string> { "56-45", "62-38", "58-41", "14-60", "19-56", "150-0"};
        private List<string> kds = new List<string> { "1.12", "1.43", "1.08", "0.61", "0.67", "5.00" };
        private List<string> fmaps = new List<string> { "Inferno", "Ancient", "Dust2", "Vertigo", "Overpass", "Hive" };

        // TeamInfo f = new TeamInfo(); important for later

        public LeaderboardCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task PullLeaderboard()
        {

            List<PageBuilder> pages = new();
            Dictionary<IEmote, PaginatorAction> emotes = new Dictionary<IEmote, PaginatorAction>();

            var backwardemote = new Emoji("\u25C0\uFE0F"); var forwardemote = new Emoji("\u25B6\uFE0F");
            emotes.Add(backwardemote, PaginatorAction.Backward); emotes.Add(forwardemote, PaginatorAction.Forward);
          
            var originalsize = teams.Count;
            for (int i = 0; i <= (originalsize / 5); i++)
            {
                string l = GenerateLeaderboardPage(teams.Take(5).ToList(), records.Take(5).ToList(), diffs.Take(5).ToList(), kds.Take(5).ToList(), fmaps.Take(5).ToList()).Result;
                pages.Add(new PageBuilder().WithTitle("Scarlet Classic's Leaderboard").WithDescription($"```{l}```").WithColor(new Color(102, 0, 0)).WithFooter("Rutgers CS:GO"));
                teams.RemoveRange(0, Math.Min(5, teams.Count)); records.RemoveRange(0, Math.Min(5, records.Count)); diffs.RemoveRange(0, Math.Min(5, diffs.Count)); kds.RemoveRange(0, Math.Min(5, kds.Count)); fmaps.RemoveRange(0, Math.Min(5, fmaps.Count));
            }
            var paginator = new StaticPaginatorBuilder().WithUsers(_context.User).WithPages(pages).WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users).WithEmotes(emotes).Build();


            await _context.Interaction.RespondAsync("retrieving leaderboard");
            var message = await _context.Interaction.GetOriginalResponseAsync();
            await _interactivity.SendPaginatorAsync(paginator, _context.Channel, TimeSpan.FromMinutes(2), message);
            await message.DeleteAsync();

        }

        public async Task<string> GenerateLeaderboardPage(List<string> teamlist, List<string> recordlist, List<string> difflist, List<string> kdlist, List<string> maplist)
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
            leaderboard += "║ " + "Team" + CalculateSpace(teamcolw, 4) + " │ " + "W/L" + CalculateSpace(recordcolw, 3) + " | " + "RD" + CalculateSpace(diffcolw, 2) + " | " + "K/D" + CalculateSpace(kdcolw, 3) + " | " + "FMap" + CalculateSpace(mapcolw, 4) + " ║" + "\r\n";  //integers are length of header titles
            leaderboard += "╠" + CalculateBorder(teamcolw, true) + "╪" + CalculateBorder(recordcolw, true) + "╪" + CalculateBorder(diffcolw, true) + "╪" + CalculateBorder(kdcolw, true) + "╪" + CalculateBorder(mapcolw, true) + "╣" + "\r\n";
        
            for (int i = 0; i < (teamlist.Count); i++)
            {
                leaderboard += "║ " + teamlist[i] + CalculateSpace(teamcolw, teamlist[i].Length) + " │ " + recordlist[i] + CalculateSpace(recordcolw, recordlist[i].Length) + " | " + difflist[i] + CalculateSpace(diffcolw, difflist[i].Length) + " | " + kdlist[i] + CalculateSpace(kdcolw, kdlist[i].Length) + " | " + maplist[i] + CalculateSpace(mapcolw, maplist[i].Length) + " ║" + "\r\n";

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
