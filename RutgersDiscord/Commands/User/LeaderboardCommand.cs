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

namespace RutgersDiscord.Commands.User
{
    public class LeaderboardCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        private string[] sampleteams = { "Team Gal", "Team Kenji", "Team August", "Team GtestBot", "Team Bozo" };
        private string[] samplerecord = { "3-2", "5-0", "4-1", "0-5", "1-4" };
        private string[] samplediff = { "56-45", "62-38", "58-41", "14-60", "19-56" };
        private string[] samplekd = { "1.12", "1.43", "1.08", "0.61", "0.67" };
        private string[] samplemap = { "Inferno", "Ancient", "Dust2", "Vertigo", "Overpass" };

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
            Task<string> leaderboard = GenerateLeaderboardPage(sampleteams, samplerecord, samplediff, samplekd, samplemap);
            string l = leaderboard.Result;
            var eb = new EmbedBuilder();
            eb.WithTitle("Scarlet Classic 2v2 Leaderboard");
            eb.WithDescription($"```{l}```");
            await _context.Interaction.RespondAsync(embed:eb.Build());

        }

        public async Task<string> GenerateLeaderboardPage(string[] teamlist, string[] recordlist, string[] difflist, string[] kdlist, string[] maplist)
        {

            int teamcolw = teamlist.Take(5).ToArray().Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
            int recordcolw = recordlist.Take(5).ToArray().Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
            int diffcolw = difflist.Take(5).ToArray().Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;
            int kdcolw = 6; // " 1.23 " format
            int mapcolw = maplist.Take(5).ToArray().Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length;

            string[] first5teams = teamlist.Take(5).ToArray();
            string[] first5records = recordlist.Take(5).ToArray();
            string[] first5diffs = difflist.Take(5).ToArray();
            string[] first5kds = kdlist.Take(5).ToArray();
            string[] first5maps = maplist.Take(5).ToArray();

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

            string header1 = "╔" + CalculateBorder(teamcolw, true) + "╤" + CalculateBorder(recordcolw, true) + "╤" + CalculateBorder(diffcolw, true) + "╤" + CalculateBorder(kdcolw, true) + "╤" + CalculateBorder(mapcolw, true) + "╗";
            string header2 = "║ " + "Team" + CalculateSpace(teamcolw, 4) + " │ " + "W/L" + CalculateSpace(recordcolw, 3) + " | " + "RD" + CalculateSpace(diffcolw, 2) + " | " + "K/D" + CalculateSpace(kdcolw, 3) + " | " + "FMap" + CalculateSpace(mapcolw, 4) + " ║";  //integers are length of header titles
            string header3 = "╠" + CalculateBorder(teamcolw, true) + "╪" + CalculateBorder(recordcolw, true) + "╪" + CalculateBorder(diffcolw, true) + "╪" + CalculateBorder(kdcolw, true) + "╪" + CalculateBorder(mapcolw, true) + "╣";

            string body1 = "║ " + first5teams[0] + CalculateSpace(teamcolw, first5teams[0].Length) + " │ " + first5records[0] + CalculateSpace(recordcolw, first5records[0].Length) + " | " + first5diffs[0] + CalculateSpace(diffcolw, first5diffs[0].Length) + " | " + first5kds[0] + CalculateSpace(kdcolw, first5kds[0].Length) + " | " + first5maps[0] + CalculateSpace(mapcolw, first5maps[0].Length) + " ║";
            string body2 = "╟" + CalculateBorder(teamcolw, false) + "┼" + CalculateBorder(recordcolw, false) + "┼" + CalculateBorder(diffcolw, false) + "┼" + CalculateBorder(kdcolw, false) + "┼" + CalculateBorder(mapcolw, false) + "╢";
            string body3 = "║ " + first5teams[1] + CalculateSpace(teamcolw, first5teams[1].Length) + " │ " + first5records[1] + CalculateSpace(recordcolw, first5records[1].Length) + " | " + first5diffs[1] + CalculateSpace(diffcolw, first5diffs[1].Length) + " | " + first5kds[1] + CalculateSpace(kdcolw, first5kds[1].Length) + " | " + first5maps[1] + CalculateSpace(mapcolw, first5maps[1].Length) + " ║";
            string body4 = "╟" + CalculateBorder(teamcolw, false) + "┼" + CalculateBorder(recordcolw, false) + "┼" + CalculateBorder(diffcolw, false) + "┼" + CalculateBorder(kdcolw, false) + "┼" + CalculateBorder(mapcolw, false) + "╢";
            string body5 = "║ " + first5teams[2] + CalculateSpace(teamcolw, first5teams[2].Length) + " │ " + first5records[2] + CalculateSpace(recordcolw, first5records[2].Length) + " | " + first5diffs[2] + CalculateSpace(diffcolw, first5diffs[2].Length) + " | " + first5kds[2] + CalculateSpace(kdcolw, first5kds[2].Length) + " | " + first5maps[2] + CalculateSpace(mapcolw, first5maps[2].Length) + " ║";
            string body6 = "╟" + CalculateBorder(teamcolw, false) + "┼" + CalculateBorder(recordcolw, false) + "┼" + CalculateBorder(diffcolw, false) + "┼" + CalculateBorder(kdcolw, false) + "┼" + CalculateBorder(mapcolw, false) + "╢";
            string body7 = "║ " + first5teams[3] + CalculateSpace(teamcolw, first5teams[3].Length) + " │ " + first5records[3] + CalculateSpace(recordcolw, first5records[3].Length) + " | " + first5diffs[3] + CalculateSpace(diffcolw, first5diffs[3].Length) + " | " + first5kds[3] + CalculateSpace(kdcolw, first5kds[3].Length) + " | " + first5maps[3] + CalculateSpace(mapcolw, first5maps[3].Length) + " ║";
            string body8 = "╟" + CalculateBorder(teamcolw, false) + "┼" + CalculateBorder(recordcolw, false) + "┼" + CalculateBorder(diffcolw, false) + "┼" + CalculateBorder(kdcolw, false) + "┼" + CalculateBorder(mapcolw, false) + "╢";
            string body9 = "║ " + first5teams[4] + CalculateSpace(teamcolw, first5teams[4].Length) + " │ " + first5records[4] + CalculateSpace(recordcolw, first5records[4].Length) + " | " + first5diffs[4] + CalculateSpace(diffcolw, first5diffs[4].Length) + " | " + first5kds[4] + CalculateSpace(kdcolw, first5kds[4].Length) + " | " + first5maps[4] + CalculateSpace(mapcolw, first5maps[4].Length) + " ║";
            string body10 = "╚" + CalculateBorder(teamcolw, true) + "╧" + CalculateBorder(recordcolw, true) + "╧" + CalculateBorder(diffcolw, true) + "╧" + CalculateBorder(kdcolw, true) + "╧" + CalculateBorder(mapcolw, true) + "╝";

            string leaderboard = header1 + "\r\n" + header2 + "\r\n" + header3 + "\r\n" + body1 + "\r\n" + body2 + "\r\n" + body3 + "\r\n" + body4 + "\r\n" + body5 + "\r\n" + body6 + "\r\n" + body7 + "\r\n" + body8 + "\r\n" + body9 + "\r\n" + body10;
            return leaderboard;



            //delete first 5 elements of data lists
            // list.RemoveRange(0,5);
        }

        
    }
}
