using Dapper.Contrib.Extensions;
using Discord.Interactions;
using System;

[Table ("match_list")]
public class MatchInfo
{
	[ExplicitKey]
	public int MatchID { get; set; }
	public int? TeamHomeID { get; set; }
	public int? TeamAwayID { get; set; }
	public long? MatchTime { get; set; }
	public int? ScoreHome { get; set; }
	public int? ScoreAway { get; set; }
	public bool? MatchFinished { get; set; }
	public bool? HomeTeamWon { get; set; }
	public long? DiscordChannel { get; set; }
	public int? MapID { get; set; }
	public bool? TeamHomeReady { get; set; }
	public bool? TeamAwayReady { get; set; }

	[ComplexParameterCtor]
	public MatchInfo(
		int id = 0, 
		int? teamHomeID = null,
		int? teamAwayID = null, 
		long? matchTime = null, 
		int? scoreHome = null, 
		int? scoreAway = null, 
		bool? matchFinished = null, 
		bool? homeTeamWon = null,
		int? mapID = null,
		string discordChannel = null,
		bool? teamHomeReady = null,
		bool? teamAwayReady = null)
    {
		Random r = new();
		MatchID = r.Next(0, int.MaxValue);
		TeamHomeID = teamHomeID;
		TeamAwayID = teamAwayID;
		MatchTime = matchTime;
		ScoreHome = scoreHome;
		ScoreAway = scoreAway;
		MatchFinished = matchFinished;
		HomeTeamWon = homeTeamWon;
		MapID = mapID;
		if (discordChannel != null)
		{
			DiscordChannel = long.Parse(discordChannel);
		}
        else
        {
			DiscordChannel = null;
        }
		TeamHomeReady = teamHomeReady;
		TeamAwayReady = teamAwayReady;
    }

	private MatchInfo()
    {

    }

	//Merge two MatchInfo objects prioritizes new over old.
	public static MatchInfo Merge(MatchInfo newMatch, MatchInfo oldMatch)
    {
		MatchInfo m = new();
        m.MatchID = newMatch.MatchID;
		m.TeamHomeID = newMatch.TeamHomeID ?? oldMatch.TeamHomeID;
		m.TeamAwayID = newMatch.TeamAwayID ?? oldMatch.TeamAwayID;
		m.MatchTime = newMatch.MatchTime ?? oldMatch.MatchTime;
		m.ScoreHome = newMatch.ScoreHome ?? oldMatch.ScoreHome;
		m.ScoreAway = newMatch.ScoreAway ?? oldMatch.ScoreAway;
		m.MatchFinished = newMatch.MatchFinished ?? oldMatch.MatchFinished;
		m.HomeTeamWon = newMatch.HomeTeamWon ?? oldMatch.HomeTeamWon;
		m.MapID = newMatch.MapID ?? oldMatch.MapID;
		m.DiscordChannel = newMatch.DiscordChannel ?? oldMatch.DiscordChannel;
		m.TeamHomeReady = newMatch.TeamHomeReady ?? oldMatch.TeamHomeReady;
		m.TeamAwayReady = newMatch.TeamAwayReady ?? oldMatch.TeamAwayReady;
		return m;
	}

    public override string ToString()
    {
        return $"MatchID: {MatchID}\nMap: {MapID}\n(H) {TeamHomeID} vs (A) {TeamAwayID}\n{ScoreHome} - {ScoreAway}\nFinished: {MatchFinished}";
    }
}
