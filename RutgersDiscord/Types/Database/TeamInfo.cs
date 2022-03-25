using Dapper.Contrib.Extensions;

[Table ("teams")]
public class TeamInfo
{
	[ExplicitKey]
	public int TeamID { get; set; }
	public string TeamName { get; set; }
	//User 1 is captain
	public long Player1 { get; set; }
	public long Player2 { get; set; }
	public int? Wins { get; set; }
	public int? Losses { get; set; }
	public int? RoundWins { get; set; }
	public int? RoundLosses { get; set; }

	private TeamInfo() { }
	public TeamInfo(int teamID, string teamName, long player1, long player2, int? wins = null, int? losses = null, int? roundWins = null, int? roundLosses = null)
    {
		TeamID = teamID;
		TeamName = teamName;
		Player1 = player1;
		Player2 = player2;
		Wins = wins;
		Losses = losses;
		RoundWins = roundWins;
		RoundLosses = roundLosses;
	}

    public override string ToString()
    {
        return $"Team Name: {TeamName}\nTeamID: {TeamID}\nPlayer 1: {Player1}\nPlayer 2: {Player2}\nWins: {Wins}\nLosses: {Losses}\nRoundWins: {RoundWins}\nRoundLosses: {RoundLosses}";
    }
}
