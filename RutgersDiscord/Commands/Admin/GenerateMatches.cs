using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using FluentScheduler;
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
    private readonly ScheduleHandler _schedule;
    private readonly ConfigHandler _config;

    public GenerateMatches(DiscordSocketClient client, SocketInteractionContext context, DatabaseHandler database, InteractivityService interactivity, ScheduleHandler schedule, ConfigHandler config)
    {
        _client = client;
        _context = context;
        _database = database;
        _interactivity = interactivity;
        _schedule = schedule;
        _config = config;
    }

    //test method
    public async Task RunAsync()
    {
        await CreateMatchChannel(await GetUsersFromMatch(await _database.GetMatchAsync(1)),"test");
    }

    public async Task CreateMatch(int teamHomeID, int teamAwayID, DateTime t, int numMatches = 1)
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
        MatchInfo match = new(0,teamHomeID: teamHomeID,teamAwayID: teamAwayID,matchTime: t.Ticks,matchFinished: false, teamHomeReady: false, teamAwayReady: false );
        List<PlayerInfo> playerList = await GetUsersFromMatch(match);
        RestTextChannel channel = await CreateMatchChannel(playerList, $"{teamHome.TeamName}_vs_{teamAway.TeamName}");
        match.DiscordChannel = (long?)channel.Id;
        await _database.AddMatchAsync(match);

        //Create Greeting message
        String greetingMessage = "Welcome ";
        foreach (PlayerInfo player in playerList)
        {
            greetingMessage += $"<@{player.DiscordID}>";
        }
        greetingMessage += " to the match room";

        //Create embed with info
        EmbedFieldBuilder defaultTime = new EmbedFieldBuilder()
            .WithName("Default Time")
            .WithValue($"<t:{(new DateTime((long)match.MatchTime).ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds}:f>")
            .WithIsInline(false);
        EmbedFieldBuilder commandList = new EmbedFieldBuilder()
            .WithName("Commands")
            .WithValue("\n`/admin`\n" +
                       "Calls an admin to your match room.\n\n" +
                       "`/reschedule [month] [day] [hour] [minute]`\n" +
                       "Propose a new time for the match.\n\n" +
                       "`/ready ... /unready`\n" +
                       "Once both teams have readied,\n" +
                       "> 1. Vetoing starts\n" +
                       "> 2. Server generates")
            .WithIsInline(false);
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle($"{teamHome.TeamName} vs. {teamAway.TeamName}")
            .AddField(defaultTime)
            .AddField(commandList);

        //Send Message
        await channel.SendMessageAsync(greetingMessage,embed: embed.Build());

        //Warning message for BO3
        if(numMatches > 1)
        {
            EmbedFieldBuilder pickBanDesc1 = new EmbedFieldBuilder()
                .WithName("Team Home VS.")
                .WithValue("Ban\n*\nPick\n*\n*\nBan")
                .WithIsInline(true);
            EmbedFieldBuilder pickBanDesc2 = new EmbedFieldBuilder()
                .WithName("Team Away")
                .WithValue("\nBan\n*\nPick\nBan");



            EmbedBuilder embedBO3 = new EmbedBuilder()
                .WithTitle("THIS MATCH IS A BEST OF 3!")
                .WithDescription("**DO NOT DISCONNECT BETWEEN MATCHES**\nBan Format")
                .WithFields(new EmbedFieldBuilder[] { pickBanDesc1, pickBanDesc2});
                
        }


        //Add job 
        if (match.MatchTime > DateTime.Now.AddMinutes(16).Ticks)
        {
            JobManager.AddJob(async () => await _schedule.MentionUsers((ulong)match.DiscordChannel, playerList.Select(s => s.DiscordID).ToList(), false), s => s.WithName($"[match_15m_{match.MatchID}]").ToRunOnceAt(new DateTime((long)match.MatchTime) - TimeSpan.FromMinutes(15)));
        }
		if (match.MatchTime > DateTime.Now.AddDays(1).Ticks)
		{
            JobManager.AddJob(async () => await _schedule.MentionUsers((ulong)match.DiscordChannel, playerList.Select(s => s.DiscordID).ToList(), true), s => s.WithName($"[match_24h_{match.MatchID}]").ToRunOnceAt(new DateTime((long)match.MatchTime) - TimeSpan.FromDays(1)));
        }

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
        overwrite.Add(new Overwrite(_config.settings.DiscordSettings.Roles.Everyone, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)));
        foreach (PlayerInfo player in playersToAdd)
        {
            overwrite.Add(new Overwrite((ulong)player.DiscordID, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)));
        }
        return await _context.Guild.CreateTextChannelAsync(channelName, c => { c.CategoryId = _config.settings.DiscordSettings.ChannelCategories.MatchesCategory; c.PermissionOverwrites = overwrite; });
    }
   
}
