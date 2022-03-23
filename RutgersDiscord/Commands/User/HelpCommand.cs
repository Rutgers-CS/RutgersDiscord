using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Pagination;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Commands.User
{
    public class HelpCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        public HelpCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task GetHelp()
        {

            List<PageBuilder> pages = new();
            Dictionary<IEmote, PaginatorAction> emotes = new Dictionary<IEmote, PaginatorAction>();

            var backwardemote = new Emoji("\u25C0\uFE0F"); var forwardemote = new Emoji("\u25B6\uFE0F");
            emotes.Add(backwardemote, PaginatorAction.Backward); emotes.Add(forwardemote, PaginatorAction.Forward);

            string gencommands = "/admin - Calls an admin" + "\r\n\r\n";
            string eventcommands = "/ready | /unready - Readies/unreadies for a match" + "\r\n\r\n" + "/leaderboard - Displays the tourney's standings" + "\r\n\r\n" + "/stats - Displays the tourney's player stats" + "\r\n\r\n";
         // /reschedule  and // /veto ------ only works in match channel, only by team captain

            EmbedBuilder embed = new EmbedBuilder().WithTitle("Rutgers CS:GO Commands").WithColor(new Color(102, 0, 0)).AddField("General Commands", $"```{gencommands}```", true).AddField("Event Commands", $"```{eventcommands}```", false).WithThumbnailUrl("https://i.imgur.com/hePqR1R.png");

            await _context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
        }

    }
}
