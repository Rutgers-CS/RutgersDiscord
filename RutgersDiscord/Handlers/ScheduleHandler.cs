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
	private readonly ConfigHandler _config;

	public ScheduleHandler(DiscordSocketClient client, DatabaseHandler database, ConfigHandler config)
	{
		_client = client;
		_database = database;
		_config = config;
		JobManager.Initialize(new Registry());
	}

	//When bot is restarted re add all jobs
	public async Task AddRequiredJobsAsync()
    {
		IEnumerable<MatchInfo> futureMatches = await _database.GetMatchByAttribute(matchFinished: false, seriesID: 1);
		//Get all players at once to not spam the database
		IEnumerable<TeamInfo> allTeams = await _database.GetAllTeamsAsync();
		foreach(MatchInfo match in futureMatches)
		{
			IEnumerable<TeamInfo> teams = allTeams.Where(s => (s.TeamID == match.TeamHomeID || s.TeamID == match.TeamAwayID));
			List<long> players = teams.Select(t => t.Player1).ToList();
			players.AddRange(teams.Select(t => t.Player2));

            try
            {
                if (match.MatchTime > DateTime.Now.AddMinutes(16).Ticks)
                {
                    JobManager.AddJob(async () => await MentionUsers((ulong)match.DiscordChannel, players, false), s => s.WithName($"[match_15m_{match.MatchID}]").ToRunOnceAt(new DateTime((long)match.MatchTime) - TimeSpan.FromMinutes(15)));
                }
                if (match.MatchTime > DateTime.Now.AddDays(1).Ticks)
                {
                    JobManager.AddJob(async () => await MentionUsers((ulong)match.DiscordChannel, players, true), s => s.WithName($"[match_24h_{match.MatchID}]").ToRunOnceAt(new DateTime((long)match.MatchTime) - TimeSpan.FromDays(1)));
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
		}
    }

	public async Task MentionUsers(ulong channel, List<long> players, bool dayAnnouncement)
    {
		string message = "heads up ";
		foreach(long player in players)
        {
			message += $"<@{player}> ";
        }
		if(!dayAnnouncement)
        {
			message += "there are 15 mins until match starts!";
		}
		else
        {
			message += "there is 1 day until match starts!";
		}
		await _client.GetGuild(_config.settings.DiscordSettings.Guild).GetTextChannel(channel).SendMessageAsync(message);
	}

	
}
