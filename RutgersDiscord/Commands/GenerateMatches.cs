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

public class GenerateMatches
{
    private readonly DiscordSocketClient _client;
    private readonly SocketInteractionContext _context;
    private readonly DatabaseHandler _database;
    private readonly InteractivityService _interactivity;

    public GenerateMatches(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity)
    {
        _client = client;
        _context = context;
        _database = database;
        _interactivity = interactivity;
    }

    //test method
    public async Task RunAsync()
    {
        await CreateMatchChannel(await GetUsersFromMatch(await _database.GetMatchAsync(1)),"test");
    }

    public async Task CreateMatch(long teamHomeID, long teamAwayID, DateTime t)
    {
        //test if match exitst
        if ((await _database.GetMatchByAttribute(teamHomeID,teamAwayID,matchFinished: false)).Count() != 0)
        {
            await _context.Interaction.RespondAsync("Match already exists",ephemeral: true);
            return;
        }

        //Get Info
        TeamInfo teamHome = await _database.GetTeamAsync(teamHomeID);
        TeamInfo teamAway = await _database.GetTeamAsync(teamAwayID);


        //Create match and channel;
        MatchInfo match = new(0, teamHomeID, teamAwayID, t.Ticks,matchFinished: false);
        List<PlayerInfo> playerList = await GetUsersFromMatch(match);
        RestTextChannel channel = await CreateMatchChannel(playerList, $"{teamHome.TeamName}_vs_{teamAway.TeamName}");
        match.DiscordChannel = (long?)channel.Id;
        await _database.AddMatchAsync(match);

        //Create Greeting message
        String greetingMessage = "Welcome ";
        foreach( PlayerInfo player in playerList)
        {
            greetingMessage += $"<@{player.DiscordID}>";
        }
        greetingMessage += " to the match page";

        //Create embed with info
        EmbedFieldBuilder commandList = new EmbedFieldBuilder()
            .WithName("Commands")
            .WithValue("`/reschedule [month] [day] [hour] [minute]` to request a match\n" +
                       "`/veto` to start veto on the map");
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle($"{teamHome.TeamName} vs. {teamAway.TeamName}")
            .AddField(commandList);

        //Send Message
        await channel.SendMessageAsync(greetingMessage,embed: embed.Build());

        //aknowledge the interaction
        await _context.Interaction.RespondAsync("Channel Created");
    }

    private async Task<List<PlayerInfo>> GetUsersFromMatch(MatchInfo match)
    {
        List<PlayerInfo> playerList = new();
        playerList.AddRange(await _database.GetPlayerByAttribute(teamID: (long)match.TeamHomeID));
        playerList.AddRange(await _database.GetPlayerByAttribute(teamID: (long)match.TeamAwayID));
        return playerList;
    }

    //Creates a match channel accessible to listed users and returns channelID
    private async Task<RestTextChannel> CreateMatchChannel(IEnumerable<PlayerInfo> playersToAdd, string channelName)
    {
        List<Overwrite> overwrite = new();
        overwrite.Add(new Overwrite(Constants.Role.everyone, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)));
        foreach (PlayerInfo player in playersToAdd)
        {
            overwrite.Add(new Overwrite((ulong)player.DiscordID, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)));
        }
        return await _context.Guild.CreateTextChannelAsync(channelName, c => { c.CategoryId = Constants.ChannelCategories.matches; c.PermissionOverwrites = overwrite; });
    }
   
}
