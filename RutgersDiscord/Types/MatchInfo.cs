using Dapper.Contrib.Extensions;
using Discord.Interactions;
using System;

[Table ("match_list")]
public class MatchInfo
{
	[ExplicitKey]
	public long MatchID { get; set; }
	public long? TeamHomeID { get; set; }
	public long? TeamAwayID { get; set; }
	public long? MatchTime { get; set; }
	public int? ScoreHome { get; set; }
	public int? ScoreAway { get; set; }
	public bool? MatchFinished { get; set; }
	public bool? HomeTeamWon { get; set; }
	public long? MapID { get; set; }

	[ComplexParameterCtor]
	public MatchInfo(long id = 0, long? teamHomeID = null, long? teamAwayID = null, long? matchTime = null, int? scoreHome = null, int? scoreAway = null, bool? matchFinished = null, bool? homeTeamWon = null, long? mapID = null)
    {
		if(id == 0)
        {
			MatchID = HelperMethods.RandomID();
		}
		else
        {
			MatchID = id;
        }
		TeamHomeID = teamHomeID;
		TeamAwayID = teamAwayID;
		MatchTime = matchTime;
		ScoreHome = scoreHome;
		ScoreAway = scoreAway;
		MatchFinished = matchFinished;
		HomeTeamWon = homeTeamWon;
		MapID = mapID;
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
		return m;
	}

    public override string ToString()
    {
        return $"MatchID: {MatchID}\nMap: {MapID}\n(H) {TeamHomeID} vs (A) {TeamAwayID}\n{ScoreHome} - {ScoreAway}\nFinished: {MatchFinished}";
    }
}
