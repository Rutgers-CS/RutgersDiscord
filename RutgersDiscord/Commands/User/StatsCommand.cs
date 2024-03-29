﻿using System;
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
    public class StatsCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        public StatsCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task GetStats(string sortBy)
        {
            string kills;
            string deaths;
            string kds;
            List<string> killslist = new();
            List<string> deathslist = new();
            List<string> kdslist = new();
            List<string> choicelist = new();
            float pk;
            float pd;
            float kdpd;
            float calckd;
            long playerdiscord = 0;

            List<string> MySort(List<string> list, bool isInt)
            {
                if (isInt)
                {
                    var ordered = list.Select(s => new { Str = s, Split = s.Split('-') })
                    .OrderByDescending(x => int.Parse(x.Split[1]))
                    .ThenBy(x => x.Split[0])
                    .Select(x => x.Str)
                    .ToList();
                    return ordered;
                }
                else
                {
                    var ordered = list.Select(s => new { Str = s, Split = s.Split('-') })
                  .OrderByDescending(x => float.Parse(x.Split[1]))
                  .ThenBy(x => x.Split[0])
                  .Select(x => x.Str)
                  .ToList();
                    return ordered;
                }
            }

            IEnumerable<PlayerInfo> dataplayers = await _database.GetAllPlayersAsync();
            foreach (var player in dataplayers)
            {

                if (player.Kills == null)
                { 
                    pk = 0f;
                } else
                {
                    pk = (float)player.Kills;
                }
                if (player.Deaths == null || player.Deaths == 0)
                {
                    pd = 0;
                    kdpd = 1f;
                } else
                {
                    pd = (float)player.Deaths;
                    kdpd = (float)player.Deaths;
                }
                
                playerdiscord = player.DiscordID;
                calckd = (float)(pk / kdpd);

                killslist.Add($"<@{playerdiscord}>" + " - " + pk);
                deathslist.Add($"<@{playerdiscord}>" + " - " + pd);
                kdslist.Add($"<@{playerdiscord}>" + " - " + calckd.ToString("0.0#"));
            }

            List<PageBuilder> pages = new();
            Dictionary<IEmote, PaginatorAction> emotes = new Dictionary<IEmote, PaginatorAction>();

            var backwardemote = new Emoji("\u25C0\uFE0F"); var forwardemote = new Emoji("\u25B6\uFE0F");
            emotes.Add(backwardemote, PaginatorAction.Backward); emotes.Add(forwardemote, PaginatorAction.Forward);

            var originalsize = killslist.Count; //all lists will be of equal size

            if (sortBy == "kd")
            {
                choicelist = MySort(kdslist, false);

                if (originalsize % 10 == 0)
                {
                    for (int i = 0; i < (originalsize / 10); i++)
                    {
                        kds = string.Join("\r\n", choicelist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Highest KD")  //"Scarlet Classic's Statistical Leaders"
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Scarlet Classic's Leaders", $"{kds}", true));   //"Highest KD"
                        choicelist.RemoveRange(0, Math.Min(10, choicelist.Count));
                    }
                }
                else
                {
                    for (int i = 0; i <= (originalsize / 10); i++)
                    {
                        kds = string.Join("\r\n", choicelist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Highest KD")
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Scarlet Classic's Leaders", $"{kds}", true));
                        choicelist.RemoveRange(0, Math.Min(10, choicelist.Count));
                    }
                }
            }
            else if (sortBy == "kills")
            {
                choicelist = MySort(killslist, true);

                if (originalsize % 10 == 0)
                {
                    for (int i = 0; i < (originalsize / 10); i++)
                    {
                        kills = string.Join("\r\n", choicelist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Most Kills")
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Scarlet Classic's Leaders", $"{kills}", true));
                        choicelist.RemoveRange(0, Math.Min(10, choicelist.Count));
                    }
                }
                else
                {
                    for (int i = 0; i <= (originalsize / 10); i++)
                    {
                        kills = string.Join("\r\n", choicelist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Most Kills")
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Scarlet Classic's Leaders", $"{kills}", true));
                        choicelist.RemoveRange(0, Math.Min(10, choicelist.Count));
                    }
                }
            }
            else if (sortBy == "deaths")
            {
                choicelist = MySort(deathslist, true);

                if (originalsize % 10 == 0)
                {
                    for (int i = 0; i < (originalsize / 10); i++)
                    {
                        deaths = string.Join("\r\n", choicelist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Most Deaths")
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Scarlet Classic's Leaders", $"{deaths}", true));
                        choicelist.RemoveRange(0, Math.Min(10, choicelist.Count));
                    }
                }
                else
                {
                    for (int i = 0; i <= (originalsize / 10); i++)
                    {
                        deaths = string.Join("\r\n", choicelist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Most Deaths")
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Scarlet Classic's Leaders", $"{deaths}", true));
                        choicelist.RemoveRange(0, Math.Min(10, choicelist.Count));
                    }
                }
            }
            else if (sortBy == "all")
            {

                killslist = MySort(killslist, true);
                deathslist = MySort(deathslist, true);
                kdslist = MySort(kdslist, false);

                if (originalsize % 10 == 0)
                {
                    for (int i = 0; i < (originalsize / 10); i++)
                    {
                        kills = string.Join("\r\n", killslist.Take(10));
                        deaths = string.Join("\r\n", deathslist.Take(10));
                        kds = string.Join("\r\n", kdslist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Scarlet Classic's Leaders")
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Most Kills", $"{kills}", true)
                            .AddField("Most Deaths", $"{deaths}", true)
                            .AddField("Highest KD", $"{kds}", true));
                        killslist.RemoveRange(0, Math.Min(10, killslist.Count));
                        deathslist.RemoveRange(0, Math.Min(10, deathslist.Count));
                        kdslist.RemoveRange(0, Math.Min(10, kdslist.Count));
                    }
                }
                else
                {
                    for (int i = 0; i <= (originalsize / 10); i++)
                    {
                        kills = string.Join("\r\n", killslist.Take(10));
                        deaths = string.Join("\r\n", deathslist.Take(10));
                        kds = string.Join("\r\n", kdslist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Scarlet Classic's Leaders")
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Most Kills", $"{kills}", true)
                            .AddField("Most Deaths", $"{deaths}", true)
                            .AddField("Highest KD", $"{kds}", true));
                        killslist.RemoveRange(0, Math.Min(10, killslist.Count));
                        deathslist.RemoveRange(0, Math.Min(10, deathslist.Count));
                        kdslist.RemoveRange(0, Math.Min(10, kdslist.Count));
                    }
                }
            } else
            {
                choicelist = MySort(kdslist, false);

                if (originalsize % 10 == 0)
                {
                    for (int i = 0; i < (originalsize / 10); i++)
                    {
                        kds = string.Join("\r\n", choicelist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Highest KD")
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Scarlet Classic's Leaders", $"{choicelist}", true));
                        choicelist.RemoveRange(0, Math.Min(10, choicelist.Count));
                    }
                }
                else
                {
                    for (int i = 0; i <= (originalsize / 10); i++)
                    {
                        kds = string.Join("\r\n", choicelist.Take(10));
                        pages.Add(new PageBuilder()
                            .WithTitle("Highest KD")
                            .WithColor(new Color(102, 0, 0))
                            .WithFooter("Rutgers CS:GO")
                            .AddField("Scarlet Classic's Leaders", $"{choicelist}", true));
                        choicelist.RemoveRange(0, Math.Min(10, choicelist.Count));
                    }
                }
            }

            var paginator = new StaticPaginatorBuilder().WithUsers(_context.User).WithPages(pages).WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users).WithEmotes(emotes).Build();

            await _context.Interaction.RespondAsync("retrieving stats");
            var message = await _context.Interaction.GetOriginalResponseAsync();
            await _interactivity.SendPaginatorAsync(paginator, _context.Channel, TimeSpan.FromMinutes(2), message);
            await message.DeleteAsync();
        }

    }

}