using Dapper.Contrib.Extensions;
using Discord.Interactions;
using System;

[Table ("matches")]
public class MatchInfo
{
	[ExplicitKey]
	public int MatchID { get; set; }
	public string DatMatchID { get; set; }
	public string ServerID { get; set; }
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
	public bool? AdminCalled { get; set; }

	[ComplexParameterCtor]
	public MatchInfo(
		int id = 0,
		string datMatchID = null,
		string serverID = null,
		int? teamHomeID = null,
		int? teamAwayID = null,
		long? matchTime = null, 
		int? scoreHome = null, 
		int? scoreAway = null, 
		bool? matchFinished = null, 
		bool? homeTeamWon = null,
		string discordChannel = null,
		int? mapID = null,
		bool? teamHomeReady = null,
		bool? teamAwayReady = null,
		bool? adminCalled = null)
    {
		Random r = new();
		MatchID = r.Next(0, int.MaxValue);
		DatMatchID = datMatchID;
		ServerID = serverID;
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
		AdminCalled = adminCalled;
    }

	private MatchInfo()
    {

    }

	//Merge two MatchInfo objects prioritizes new over old.
	public static MatchInfo Merge(MatchInfo newMatch, MatchInfo oldMatch)
    {
		MatchInfo m = new();
        m.MatchID = newMatch.MatchID;
		m.DatMatchID = newMatch.DatMatchID ?? oldMatch.DatMatchID;
		m.ServerID = newMatch.ServerID ?? oldMatch.ServerID;
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
		m.AdminCalled = newMatch.AdminCalled ?? oldMatch.AdminCalled;
		return m;
	}

    public override string ToString()
    {
        return $"MatchID: {MatchID}\nMap: {MapID}\n(H) {TeamHomeID} vs (A) {TeamAwayID}\n{ScoreHome} - {ScoreAway}\nFinished: {MatchFinished}";
    }
}
