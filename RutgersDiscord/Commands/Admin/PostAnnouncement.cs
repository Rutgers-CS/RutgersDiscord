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

namespace RutgersDiscord.Commands.Admin
{
    public class PostAnnouncement
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketInteractionContext _context;
        private readonly DatabaseHandler _database;
        private readonly InteractivityService _interactivity;

        public PostAnnouncement(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
        {
            _client = client;
            _context = context;
            _database = database;
            _interactivity = interactivity;
        }

        public async Task GetAnnouncement()
        {
            ulong discid = Constants.Channels.announcements; // change to announcement channel 
            ulong roleid = Constants.Role.goodfellas; //change to goodfellas role
            var everyone = _client.GetGuild(Constants.guild).EveryoneRole.Mention; //everyone
            var chnl = _client.GetChannel(discid) as IMessageChannel;

            var builder = new ComponentBuilder()
                .WithButton("Register Today", "spawn_registration_form", emote: new Emoji("▶"), style: ButtonStyle.Success);

            await chnl.SendFileAsync(new FileAttachment("./coin.gif"));

            string input = ":mega:  **Scarlet Classic VIII Wingman 2v2 Tournament** " + $"<@&{roleid}>" + " " + $"{everyone}" + "\r\n" + "\r\n";
            input += ":ballot_box_with_check:  Open to ***EVERYONE*** (Other Universities and Alumni Included)" + "\r\n" + "\r\n";
            input += ":ballot_box_with_check:  Free Entry" + "\r\n" + "\r\n";
            input += ":gift:  Prizing by Rutgers Counter Strike and Rutgers Esports" + "\r\n" + "\r\n";
            input += ":trophy:  March 26th -> April 23rd" + "\r\n" + "\r\n";
            input += "> •  Swiss-System Tournament - 4 Rounds - 2 Matches per Week" + "\r\n";
            input += "> •  BO1 MR12 (First to 13) Overtime On" + "\r\n";
            input += "> •  Top teams advance to bracket play" + "\r\n";
            input += "> •  **LAN Semis and Finals**" + "\r\n";
            input += "> •  BO3 MR12 Overtime On" + "\r\n";
            input += ">    (Your team must be able to make it to campus for LAN matches on April 23rd if you qualify)" + "\r\n" + "\r\n";
            input += ":robot:  Matches Will Be Fully Automated" + "\r\n";
            input += "        Play Whenever You Want (So long as both teams agree)" + "\r\n" + "\r\n";
            input += ":shrug:  Don't Have a Partner? We'll help you find one just select Looking for Team during registration" + "\r\n" + "\r\n";
            input += ":book:  Full Rules & Details: https://bit.ly/3Inj701" + "\r\n" + "\r\n";
            input += "> **Registration Ends Friday March 25th at Midnight**" + "\r\n";
            input += "> **More details in the next few days**" + "\r\n";
            var msg = await chnl.SendMessageAsync(input);
            await msg.ModifyAsync(x => x.Components = builder.Build());

            var chnl2 = _client.GetChannel(Constants.Channels.scAnnoucement) as IMessageChannel;
            await chnl2.SendFileAsync(new FileAttachment("./coin.gif"));
            var msg2 = await chnl2.SendMessageAsync(input);
            await msg2.ModifyAsync(x => x.Components = builder.Build());

            await _context.Interaction.RespondAsync("Posted announcement successfully", ephemeral: true);
        }

    }

}