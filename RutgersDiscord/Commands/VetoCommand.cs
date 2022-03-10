using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
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

    public async Task StartVeto()
    {
        //Test for captain
        TeamInfo team = _database.GetTeamByUser(_context.User.Id, true);
        if(team == null)
        {
            await _context.Interaction.RespondAsync("User is not captain of a team");
            return;
        }

        //Find match
        var matchList = _database.GetMatchInfo(_context.User.Id, false);

        //no match found
        if (matchList == null)
        {
            await _context.Interaction.RespondAsync("Match not found");
            return;
        }
        MatchInfo match = matchList.First();

        //no match found
        if(match == null)
        {
            await _context.Interaction.RespondAsync("Match not found");
            return;
        }
        //map is selected
        if(match.Map != null)
        {
            await _context.Interaction.RespondAsync("Veto was already done");
            return;
        }

        //TODO send confirmation to captain and create embed
        ComponentBuilder component = new ComponentBuilder()
                .WithButton("Start Veto", "veto_accept");
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Waiting for opponent to accept");
        await _context.Interaction.RespondAsync(embed: embed.Build(),components: component.Build());

        //await _context.Interaction.ModifyOriginalResponseAsync(m => m.);

        //Set Home and Away teams
        TeamInfo teamHome, teamAway;
        if(team.TeamID == match.TeamHome)
        {
            teamHome = team;
            teamAway = _database.GetTeamById((ulong)match.TeamAway);
            var temp = await _interactivity.NextButtonAsync(u => (long)u.User.Id == teamAway.Player1);
            await temp.Value.DeferAsync();
        }
        else
        {
            teamAway = team;
            teamHome = _database.GetTeamById((ulong)match.TeamHome);
            var temp = await _interactivity.NextButtonAsync(u => (long)u.User.Id == teamAway.Player1);
            await temp.Value.DeferAsync();
        }

        embed.WithTitle("Veto starting");
        ComponentBuilder emptyComponent = new ComponentBuilder();
        await _context.Interaction.ModifyOriginalResponseAsync(m => { m.Embed = embed.Build();m.Components = emptyComponent.Build(); });  

        //await _context.Interaction.RespondAsync("end");

    }
}
