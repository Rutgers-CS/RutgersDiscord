using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Interactivity;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
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

    public async Task RunAsync()
    {
        await CreateMatchChannel(GetUsersFromMatch(_database.GetMatchById(1)));
    }

    public async Task CreateMatch(long teamHomeID, long teamAwayID, DateTime t)
    {
        MatchInfo match = new(0, teamHomeID, teamAwayID, t.Ticks,matchFinished: false);
        RestTextChannel channel = await CreateMatchChannel(GetUsersFromMatch(match));
        match.DiscordChannel = (long?)channel.Id;
        _database.AddMatch(match);
    }

    private List<PlayerInfo> GetUsersFromMatch(MatchInfo match)
    {
        List<PlayerInfo> playerList = new();
        playerList.AddRange(_database.GetPlayerByTeam((long)match.TeamHome));
        playerList.AddRange(_database.GetPlayerByTeam((long)match.TeamAway));
        return playerList;
    }

    //Creates a match channel accessible to listed users and returns channelID
    private async Task<RestTextChannel> CreateMatchChannel(IEnumerable<PlayerInfo> playersToAdd)
    {
        List<Overwrite> overwrite = new();
        overwrite.Add(new Overwrite(Constants.Role.everyone, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)));
        foreach (PlayerInfo player in playersToAdd)
        {
            overwrite.Add(new Overwrite((ulong)player.DiscordID, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)));
        }
        return await _context.Guild.CreateTextChannelAsync("MatchTest", c => { c.CategoryId = Constants.ChannelCategories.matches; c.PermissionOverwrites = overwrite; });
    }
   
}
