using Dapper.Contrib.Extensions;
using Discord.Interactions;
using System;

[Table ("match_list")]
public class MatchInfo
{
	[ExplicitKey]
	public long ID { get; set; }
	public long? TeamHome { get; set; }
	public long? TeamAway { get; set; }
	public long? MatchTime { get; set; }
	public int? ScoreHome { get; set; }
	public int? ScoreAway { get; set; }
	public bool? MatchFinished { get; set; }
	public bool? HomeTeamWon { get; set; }
	public string Map { get; set; }
	public long? DiscordChannel { get; set; }

	[ComplexParameterCtor]
	public MatchInfo(long id = 0, long? teamHomeID = null, long? teamAwayID = null, long? matchTime = null, int? scoreHome = null, int? scoreAway = null, bool? matchFinished = null, bool? homeTeamWon = null, string map = null, long? discordChannel = null)
    {
		if(id == 0)
        {
			ID = HelperMethods.RandomID();
		}
		else
        {
			ID = id;
        }
		TeamHome = teamHomeID;
		TeamAway = teamAwayID;
		MatchTime = matchTime;
		ScoreHome = scoreHome;
		ScoreAway = scoreAway;
		MatchFinished = matchFinished;
		HomeTeamWon = homeTeamWon;
		Map = map;
		DiscordChannel = discordChannel;
    }

	private MatchInfo()
    {

    }

	//Merge two MatchInfo objects prioritizes new over old.
	public static MatchInfo Merge(MatchInfo newMatch, MatchInfo oldMatch)
    {
		MatchInfo m = new();
        m.ID = newMatch.ID;
		m.TeamHome = newMatch.TeamHome ?? oldMatch.TeamHome;
		m.TeamAway = newMatch.TeamAway ?? oldMatch.TeamAway;
		m.MatchTime = newMatch.MatchTime ?? oldMatch.MatchTime;
		m.ScoreHome = newMatch.ScoreHome ?? oldMatch.ScoreHome;
		m.ScoreAway = newMatch.ScoreAway ?? oldMatch.ScoreAway;
		m.MatchFinished = newMatch.MatchFinished ?? oldMatch.MatchFinished;
		m.HomeTeamWon = newMatch.HomeTeamWon ?? oldMatch.HomeTeamWon;
		m.Map = newMatch.Map ?? oldMatch.Map;
		m.DiscordChannel = newMatch.DiscordChannel ?? oldMatch.DiscordChannel;
		return m;
	}
}
