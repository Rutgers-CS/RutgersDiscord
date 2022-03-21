using System;
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
using Interactivity.Pagination;

namespace RutgersDiscord.Commands.User
{
    public class StatsCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        private List<string> killslist = new List<string> { "Kenji - 67", "August - 53", "Gal - 45", "Pat - 41", "Matt - 23", "James - 21", "Pike - 18", "Lucas - 8", "Buttermilk - 5", "Jdawg - 2", "Poopyhead - 1" };
        private List<string> deathslist = new List<string> { "Natedogg - 59", "Jdawg - 50", "Gal - 49", "Jesse - 43", "Mustard - 26", "James - 21", "Pike - 18", "Lucas - 8", "Buttermilk - 5", "Poopyhead - 1", "Chalk - 0" };
        private List<string> kdslist = new List<string> { "Kenji - 1.45", "August - 1.42", "Gal - 1.21", "Pat - 1.19", "Matt - 1.01", "James - 1.00", "Pike - 0.93", "Lucas - 0.85", "Buttermilk - 0.67", "Jdawg - 0.43", "Poopyhead - 0.12" };


        public StatsCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task GetStats()
        {
            string kills;
            string deaths;
            string kds;
            List<PageBuilder> pages = new();
            Dictionary<IEmote, PaginatorAction> emotes = new Dictionary<IEmote, PaginatorAction>();

            var backwardemote = new Emoji("\u25C0\uFE0F"); var forwardemote = new Emoji("\u25B6\uFE0F");
            emotes.Add(backwardemote, PaginatorAction.Backward); emotes.Add(forwardemote, PaginatorAction.Forward);

            var originalsize = killslist.Count; //all lists will be of equal size
            for (int i = 0; i <= (originalsize / 10); i++)
            {
                kills = string.Join("\r\n", killslist.Take(10));
                deaths = string.Join("\r\n", deathslist.Take(10));
                kds = string.Join("\r\n", kdslist.Take(10));
                pages.Add(new PageBuilder().WithTitle("Scarlet Classic's Statistical Leaders").WithColor(new Color(102, 0, 0)).WithFooter("Rutgers CS:GO").AddField("Most Kills", $"```{kills}```", true).AddField("Most Deaths", $"```{deaths}```", true).AddField("Highest KD", $"```{kds}```", false).WithThumbnailUrl("https://imgur.com/Fb2i1rI.png"));
                killslist.RemoveRange(0, Math.Min(10, killslist.Count));
                deathslist.RemoveRange(0, Math.Min(10, deathslist.Count));
                kdslist.RemoveRange(0, Math.Min(10, kdslist.Count));
            }
            var paginator = new StaticPaginatorBuilder().WithUsers(_context.User).WithPages(pages).WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users).WithEmotes(emotes).Build();

            await _context.Interaction.RespondAsync("retrieving stats");
            var message = await _context.Interaction.GetOriginalResponseAsync();
            await _interactivity.SendPaginatorAsync(paginator, _context.Channel, TimeSpan.FromMinutes(2), message);
            await message.DeleteAsync();
        }

    }

}