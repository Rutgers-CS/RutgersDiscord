using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
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
        //Test for captain
        TeamInfo team = await _database.GetTeamByDiscordIDAsync((long)_context.User.Id);
        if(team.Player1 != (long)_context.User.Id)
        {
            await _context.Channel.SendMessageAsync("User is not captain of a team");
            return;
        }
        //Find match
        MatchInfo match = (await _database.GetMatchByAttribute(teamID1: team.TeamID, matchFinished: false,discordChannel: (long?)_context.Channel.Id)).FirstOrDefault();

        //no match found
        if(match == null)
        {
            await _context.Channel.SendMessageAsync("Match not found");
            return;
        }
        //map is selected
        if(match.MapID != null)
        {
            await _context.Channel.SendMessageAsync("Veto was already done");
            return;
        }

        //TODO send confirmation to captain and create embed
        ComponentBuilder component = new ComponentBuilder()
                .WithButton("Start Veto", $"veto_accept_{match.MatchID}");
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Waiting for opponent to accept");
        RestUserMessage message = await _context.Channel.SendMessageAsync(embed: embed.Build(),components: component.Build());

        //await _context.Interaction.ModifyOriginalResponseAsync(m => m.);

        //Set Home and Away teams
        TeamInfo teamHome, teamAway;
        if(team.TeamID == match.TeamHomeID)
        {
            teamHome = team;
            teamAway = await _database.GetTeamAsync((int)match.TeamAwayID);
            var temp = await _interactivity.NextButtonAsync(u => (long)u.User.Id == teamAway.Player1
                && ((SocketMessageComponent)u).Data.CustomId == $"veto_accept_{match.MatchID}");
            await temp.Value.DeferAsync();
        }
        else
        {
            teamAway = team;
            teamHome = await _database.GetTeamAsync((int)match.TeamHomeID);
            var temp = await _interactivity.NextButtonAsync(u => (long)u.User.Id == teamAway.Player1
                && ((SocketMessageComponent)u).Data.CustomId == $"veto_accept_{match.MatchID}");
            await temp.Value.DeferAsync();
        }

        //Get captains for quick reference
        SocketUser captainHome = _context.Guild.GetUser((ulong)teamHome.Player1);
        SocketUser captainAway = _context.Guild.GetUser((ulong)teamAway.Player1);

       //Clear button from message
        ComponentBuilder emptyComponent = new ComponentBuilder();
        await message.ModifyAsync(m => { m.Embed = embed.Build(); m.Components = emptyComponent.Build(); });

        //Start veto
        List<MapInfo> mapPool = (await _database.GetAllMapsAsync()).ToList();
        var currentTurn = captainHome;
        int mapsRemaining = mapPool.Count;
        bool[] banCaptainHome = new bool[mapPool.Count];
        bool[] banCaptainAway = new bool[mapPool.Count];
        while (mapsRemaining > 1)
        {
            embed = EmbedHelper(captainHome, captainAway, currentTurn, mapPool, banCaptainHome, banCaptainAway);
            ComponentBuilder dropDownMenu = DropDownMenuHelper(match, mapPool, banCaptainHome, banCaptainAway);

            //updates the message
            await message.ModifyAsync(m => { m.Embed = embed.Build(); m.Components = dropDownMenu.Build(); });

            var reply = await _interactivity.NextInteractionAsync(filter: s => s.User.Id == currentTurn.Id
                && ((SocketMessageComponent)s).Data.CustomId == $"veto_main_{match.MatchID}");
            await reply.Value.DeferAsync();

            int selection = int.Parse(((reply.Value as SocketMessageComponent).Data.Values as String[])[0]);

            if (currentTurn == captainHome)
            {
                banCaptainHome[selection] = true;
            }
            else
            {
                banCaptainAway[selection] = true;
            }

            if (currentTurn == captainHome)
            {
                currentTurn = captainAway;
            }
            else
            {
                currentTurn = captainHome;
            }

            mapsRemaining--;
        }

        //Veto Finished
        string mapName = "";
        for (int i = 0; i < mapPool.Count; i++)
        {
            if (!(banCaptainHome[i] || banCaptainAway[i]))
            {
                mapName = mapPool[i].MapName;
                break;
            }
        }

        //TODO: add map image
        EmbedBuilder embedPost = new EmbedBuilder()
            .WithColor(Constants.EmbedColors.reject)
            .WithTitle($"{teamHome.TeamName} VS {teamAway.TeamName}")
            .WithDescription($"⠀\n Map:\n{mapName}")
            .WithImageUrl(Constants.ImgurAlbum[mapName]);


        await message.ModifyAsync(m => { m.Embed = embedPost.Build(); m.Components = emptyComponent.Build();});
        MapInfo map = await _database.GetMapByNameAsync(mapName);
        match.MapID = map.MapID;
        await _database.UpdateMatchAsync(match);
    }


    private EmbedBuilder EmbedHelper(SocketUser captainHome, SocketUser captainAway, SocketUser currentTurn, List<MapInfo> mapPool, bool[] banCaptainHome, bool[] banCaptainAway)
    {
        EmbedBuilder embed = new EmbedBuilder()
            .WithColor(Constants.EmbedColors.active)
            .WithDescription($"{currentTurn.Username} turn to ban");

        string checkmark = ":negative_squared_cross_mark:";
        int count = 0;
        string embedColumn1 = "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n";
        string embedColumn2 = "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n";
        string embedColumn3 = "⠀\n";
        foreach (MapInfo map in mapPool)
        {
            embedColumn1 += $"{count + 1} - {(banCaptainHome[count] ? checkmark : "")}\n";
            embedColumn2 += $"{map.MapName}\n";
            embedColumn3 += $"{(banCaptainAway[count] ? checkmark : "")}\n";
            count++;
        }

        List<EmbedFieldBuilder> f = new List<EmbedFieldBuilder>();
        f.Add(new EmbedFieldBuilder()
            .WithName($"⠀⠀Team {captainHome.Username}")
            .WithValue(embedColumn1)
            .WithIsInline(true));
        f.Add(new EmbedFieldBuilder()
            .WithName($"VS")
            .WithValue(embedColumn2)
            .WithIsInline(true));
        f.Add(new EmbedFieldBuilder()
            .WithName($"Team {captainAway.Username}")
            .WithValue(embedColumn3)
            .WithIsInline(true));
        embed.WithFields(f);

        return embed;
    }

    private ComponentBuilder DropDownMenuHelper(MatchInfo match, List<MapInfo> mapPool, bool[] banCaptainHome, bool[] banCaptainAway)
    {
        List<SelectMenuOptionBuilder> s = new List<SelectMenuOptionBuilder>();
        int count = 0;
        foreach (MapInfo map in mapPool)
        {
            if (!(banCaptainHome[count] || banCaptainAway[count]))
            {
                s.Add(new SelectMenuOptionBuilder(label: map.MapName, value: count.ToString()));
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
}
