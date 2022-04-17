using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class VetoCommand
{
    private readonly DiscordSocketClient _client;
    private readonly SocketInteractionContext _context;
    private readonly DatabaseHandler _database;
    private readonly InteractivityService _interactivity;

    public VetoCommand(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
    {
        _client = client;
        _context = context;
        _database = database;
        _interactivity = interactivity;
    }

    public async Task StartVetoAcknowledge()
    {
        await _context.Interaction.DeferAsync();
        await StartVeto();
        await _context.Interaction.DeleteOriginalResponseAsync();
    }

    public async Task StartVeto()
    {
        IEnumerable<MatchInfo> matches = await _database.GetMatchByAttribute(matchFinished: false, discordChannel: (long?)_context.Channel.Id);
        //Specifies voting order. True for team home, false for team away
        bool[] voteOrder;
        //Specifies if its a pick or ban. Pick -> true, Ban -> false
        bool[] pickBan;
        if (matches.Count() == 1)
        {
            voteOrder = new []{ true, false, true, false, true, false };
            pickBan = new []{ false, false, false, false, false, false };
        }
        else
        {
            voteOrder = new [] { true, false, true, false, true, false };
            pickBan = new [] { false, false, true, true, false, false };
        }
        await Veto(voteOrder, pickBan);
    }

    private async Task Veto(bool[] voteOrder, bool[] pickBan)
    {
        //Test for captain
        TeamInfo team = await _database.GetTeamByDiscordIDAsync((long)_context.User.Id);
        if (team.Player1 != (long)_context.User.Id)
        {
            await _context.Channel.SendMessageAsync("User is not captain of a team");
            return;
        }
        //Find match
        IEnumerable<MatchInfo> matches = await _database.GetMatchByAttribute(teamID1: team.TeamID, matchFinished: false, discordChannel: (long?)_context.Channel.Id);
        matches = matches.OrderBy(m => m.SeriesID);
        //TODO maybe change First to Where(m => m.map == 1).First();
        MatchInfo match = matches.FirstOrDefault();

        //no match found
        if (match == null)
        {
            await _context.Channel.SendMessageAsync("Match not found");
            return;
        }
        //map is selected
        if (match.MapID != null)
        {
            await _context.Channel.SendMessageAsync("Veto was already done");
            return;
        }

        //Set Home and Away teams
        TeamInfo teamHome, teamAway;
        long opponent;
        if (team.TeamID == match.TeamHomeID)
        {
            teamHome = team;
            teamAway = await _database.GetTeamAsync((int)match.TeamAwayID);
            opponent = teamAway.Player1;
        }
        else
        {
            teamAway = team;
            teamHome = await _database.GetTeamAsync((int)match.TeamHomeID);
            opponent = teamHome.Player1;
        }

        //Send confirmation for opponent to start the veto
        ComponentBuilder component = new ComponentBuilder()
                .WithButton("Start Veto", $"veto_accept_{match.MatchID}");
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle($"Waiting for {_context.Guild.GetUser((ulong)opponent).Username} to accept");
        RestUserMessage message = await _context.Channel.SendMessageAsync(embed: embed.Build(), components: component.Build());
        var temp = await _interactivity.NextButtonAsync(u => (long)u.User.Id == opponent
            && ((SocketMessageComponent)u).Data.CustomId == $"veto_accept_{match.MatchID}");
        await temp.Value.DeferAsync();

        //Get captains for quick reference
        SocketUser captainHome = _context.Guild.GetUser((ulong)teamHome.Player1);
        SocketUser captainAway = _context.Guild.GetUser((ulong)teamAway.Player1);

        //Clear button from message
        await message.ModifyAsync(m => { m.Embed = embed.Build(); m.Components = null; });

        //Start veto
        List<MapInfo> mapPool = (await _database.GetAllMapsAsync()).ToList();
        bool?[] banCaptainHome = new bool?[mapPool.Count];
        bool?[] banCaptainAway = new bool?[mapPool.Count];
        int[] mapOrder = new int[mapPool.Count];
        List<MapInfo> mapSelection = new List<MapInfo>();
        for(int i = 0; i < mapPool.Count - 1; i++)
        {
            SocketUser currentTurn = voteOrder[i] ? captainHome : captainAway;
            embed = EmbedHelper(teamHome.TeamName, teamAway.TeamName, currentTurn, mapPool, banCaptainHome, banCaptainAway, mapOrder, pickBan[i]);
            ComponentBuilder dropDownMenu = DropDownMenuHelper(match, mapPool,banCaptainHome,banCaptainAway);

            //updates the message
            await message.ModifyAsync(m => { m.Embed = embed.Build(); m.Components = dropDownMenu.Build(); });

            var reply = await _interactivity.NextInteractionAsync(filter: s => s.User.Id == currentTurn.Id
                && ((SocketMessageComponent)s).Data.CustomId == $"veto_main_{match.MatchID}");
            await reply.Value.DeferAsync();

            SocketMessageComponentData replyData = (reply.Value as SocketMessageComponent).Data;
            int selection = int.Parse(replyData.Values.First().Split(" ")[0]);
            string selectionName = replyData.Values.First().Split(" ")[1];

            if (currentTurn == captainHome)
            {
                banCaptainHome[selection] = pickBan[i];
            }
            else
            {
                banCaptainAway[selection] = pickBan[i];
            }

            if(pickBan[i])
            {
                mapSelection.AddRange(mapPool.Where(s => s.MapName == selectionName));
                mapOrder[selection] = mapSelection.Count;
            }
        }
        //Add LeftoverMap
        for (int i = 0; i < mapPool.Count; i++)
        {
            if (banCaptainAway[i] == null && banCaptainHome[i] == null)
            {
                mapSelection.Add(mapPool[i]);
                break;
            }
        }



        string embedDescription = "Maps: \n";
        int count = 1;
        foreach (var map in mapSelection)
        {
            embedDescription += $"{count++}. {map.MapName}\n";
        }

        //Veto Finished
        EmbedBuilder embedPost = new EmbedBuilder()
            .WithColor(Constants.EmbedColors.reject)
            .WithTitle($"{teamHome.TeamName} VS {teamAway.TeamName}")
            .WithDescription(embedDescription)
            .WithImageUrl(Constants.ImgurAlbum[mapSelection.First().MapName]);
        await message.ModifyAsync(m => { m.Embed = embedPost.Build(); m.Components = null; });


        foreach (var tuple in mapSelection.Zip(matches,(map,match) => (map,match)))
        {
            tuple.match.MapID =  tuple.map.MapID;
            await _database.UpdateMatchAsync(tuple.match);
        }
    }
    private EmbedBuilder EmbedHelper(string homeName, string awayName, SocketUser currentTurn, List<MapInfo> mapPool, bool?[] banCaptainHome, bool?[] banCaptainAway, int[] mapOrder, bool pickOrBan)
    {
        string voteMode = pickOrBan ? "PICK" : "BAN";
        EmbedBuilder embed = new EmbedBuilder()
            .WithColor(Constants.EmbedColors.active)
            .WithDescription($"**{currentTurn.Username}** turn to **{voteMode}**");

        string crossmark = ":negative_squared_cross_mark:";
        int count = 0;
        string embedColumn1 = "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n";
        string embedColumn2 = "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n";
        string embedColumn3 = "⠀\n";
        foreach (MapInfo map in mapPool)
        {
            string homePick = "";
            string awayPick = "";
            if (banCaptainHome[count] == true) homePick = GetNumberEmoji(mapOrder[count]);
            else if (banCaptainHome[count] == false) homePick = crossmark;
            if (banCaptainAway[count] == true) awayPick = GetNumberEmoji(mapOrder[count]);
            else if (banCaptainAway[count] == false) awayPick = crossmark;

            embedColumn1 += $"{count + 1} - {homePick}\n";
            embedColumn2 += $"{map.MapName}\n";
            embedColumn3 += $"{awayPick}\n";
            count++;
        }

        List<EmbedFieldBuilder> f = new List<EmbedFieldBuilder>();
        f.Add(new EmbedFieldBuilder()
            .WithName($"{homeName}")
            .WithValue(embedColumn1)
            .WithIsInline(true));
        f.Add(new EmbedFieldBuilder()
            .WithName($"VS")
            .WithValue(embedColumn2)
            .WithIsInline(true));
        f.Add(new EmbedFieldBuilder()
            .WithName($"{awayName}")
            .WithValue(embedColumn3)
            .WithIsInline(true));
        embed.WithFields(f);

        return embed;
    }

    private ComponentBuilder DropDownMenuHelper(MatchInfo match, List<MapInfo> mapPool,bool?[] teamHomeBan, bool?[] teamAwayBan)
    {
        List<SelectMenuOptionBuilder> s = new List<SelectMenuOptionBuilder>();
        int count = 0;
        foreach (MapInfo map in mapPool)
        {
            if(teamHomeBan[count] == null && teamAwayBan[count] == null)
            {
                s.Add(new SelectMenuOptionBuilder(label: map.MapName, value: count.ToString() + " " + map.MapName));
            }
            count++;
        }

        SelectMenuBuilder selectMenu = new SelectMenuBuilder()
                .WithCustomId($"veto_main_{match.MatchID}")
                .WithPlaceholder("Select Map Here")
                .WithOptions(s);

        ComponentBuilder dropDownMenu = new ComponentBuilder()
            .WithSelectMenu(selectMenu);

        return dropDownMenu;
    }

    private string GetNumberEmoji(int input)
    {
        string output = "";
        while (input > 0)
        {
            int digit = input % 10;
            input /= 10;
            switch (digit)
            {
                case 0: output = ":zero:" + output; break;
                case 1: output = ":one:" + output; break;
                case 2: output = ":two:" + output; break;
                case 3: output = ":three:" + output; break;
                case 4: output = ":four:" + output; break;
                case 5: output = ":five:" + output; break;
                case 6: output = ":six:" + output; break;
                case 7: output = ":seven:" + output; break;
                case 8: output = ":eight:" + output; break;
                case 9: output = ":nine:" + output; break;
            }
        }
        return output;
    }
}
