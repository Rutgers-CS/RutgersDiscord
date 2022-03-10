public class MatchInfo
{
	public long TeamHome { get; set; }
	public long TeamAway { get; set; }
	public long MatchTime { get; set; }
	public int? ScoreHome { get; set; }
	public int? ScoreAway { get; set; }
	public bool MatchFinished { get; set; }
	public bool HomeTeamWon { get; set; }
	public string Map { get; set; }
}
