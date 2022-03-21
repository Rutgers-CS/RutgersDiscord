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

namespace RutgersDiscord.Commands.User
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
            ulong discid = 860410058961059890; // change to announcement channel 
            ulong roleid = 955316333560602624; //change to goodfellas role
            var everyone = _client.GetGuild(670683408057237547).EveryoneRole.Mention; //everyone
            var chnl = _client.GetChannel(discid) as IMessageChannel;

            string input = ":mega:  **Scarlet Classic Wingman 2v2 Tournament** " + $"<@&{roleid}>" + " " + $"{everyone}" + "\r\n" + "\r\n" + ":ballot_box_with_check:  Prizing by RUCS and Rutgers Esports" + "\r\n" + "\r\n" + ":ballot_box_with_check:  Free entry" +"\r\n" + "\r\n";
            input += ":ballot_box_with_check:  Open to EVERYONE" + "\r\n" + "\r\n" + ":star:  In-person Semis / Final at Scarlet Classic" + "\r\n" + "\r\n" + ":trophy:" + "\r\n" + "> •  Tournament kicks off on **Thursday, March 24th** with two (2) **weekly** swiss system matches that can be played at ANY time (upon agreement with opponent)." + "\r\n" + "> " + "\r\n";
            input += "> •  The top % enter **bracket play** on Thursday, April 21st - Saturday, April 23rd, where on Sunday our remaining teams are required to play the **streamed** semifinals / finals at the Scarlet Classic on April 23rd. Note: The % for bracket play will be decided later." + "\r\n" + "> " + "\r\n";
            input += "> •  Swiss rounds are BO1 MR12 (first to 13 rounds). Bracket play is BO3 MR12. Overtime is on." + "\r\n" + "> " + "\r\n";
            input += "> •  Don't have a partner? We'll help you find one, just register and click the button 'Looking for Team' in your DM." + "\r\n" + " > " + "\r\n";
            input += "> •  All tournament information: https://docs.google.com/document/d/1gbj2JsKsoJKbh5ljHNLRdrSk4_sQmVv4_RX-u53UyyA/edit?usp=sharing" + "\r\n" + "\r\n";
            input += ":pencil:  SIGN-UP BELOW" + "\r\n";
            await chnl.SendMessageAsync(input);
            await _context.Interaction.RespondAsync("Posted announcement successfully", ephemeral: true);

        }

    }

}