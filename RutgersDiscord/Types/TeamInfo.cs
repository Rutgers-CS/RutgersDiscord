using Dapper.Contrib.Extensions;

[Table ("team_list")]
public class TeamInfo
{
	public string TeamName { get; set; }
	[ExplicitKey]
	public long TeamID { get; set; }
	//User 1 is captain
	public long Player1 { get; set; }
	public long Player2 { get; set; }
}
