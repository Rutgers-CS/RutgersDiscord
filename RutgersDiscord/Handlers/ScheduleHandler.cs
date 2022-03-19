using FluentScheduler;
using RutgersDiscord.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;
using Discord.Rest;

public class ScheduleHandler
{
	private readonly DiscordSocketClient _client;
	private readonly DatabaseHandler _database;

	public ScheduleHandler(DiscordSocketClient client, DatabaseHandler database)
	{
		_client = client;
		_database = database;
		JobManager.Initialize(new Registry());
	}

	//When bot is restarted re add all jobs
	public async Task AddRequiredJobsAsync()
    {
		//TODO
		IEnumerable<MatchInfo> futureMatches = await _database.GetMatchByAttribute(matchFinished: false);
		//Get all players at once to not spam the database
		IEnumerable<TeamInfo> allTeams = await _database.GetAllTeamsAsync();
		foreach(MatchInfo match in futureMatches)
        {
			IEnumerable<TeamInfo> teams = allTeams.Where(s => (s.TeamID == match.TeamHomeID || s.TeamID == match.TeamAwayID));
			List<long> players = teams.Select(t => t.Player1).ToList();
			players.AddRange(teams.Select(t => t.Player2));
			
			JobManager.AddJob(async () => await MentionUsers(match),s => s.ToRunOnceAt(new DateTime((long)match.MatchTime) - TimeSpan.FromMinutes(15)));
        }
    }

	private async Task MentionUsers(MatchInfo match)
    {
		RestTextChannel channel = (RestTextChannel)await _client.GetChannelAsync((ulong)match.DiscordChannel);
    }

	
}
