using Dapper.Contrib.Extensions;
[Table ("players")]
public class PlayerInfo
{
    [ExplicitKey]
    public long DiscordID { get; set; }
    public long Steam64ID { get; set; }
    public string SteamID { get; set; }
    public string Name { get; set; }
    public int? TeamID { get; set; }
    public int? Kills { get; set; }
    public int? Deaths { get; set; }

    private PlayerInfo() { }
    public PlayerInfo(long DiscordID, long Steam64ID, string SteamID, string Name, int? TeamID = null, int? Kills = null, int? Deaths = null)
    {
        this.DiscordID = DiscordID;
        this.Steam64ID = Steam64ID;
        this.SteamID = SteamID;
        this.Name = Name;
        this.TeamID = TeamID;
        this.Kills = Kills;
        this.Deaths = Deaths;
    }

    public override string ToString()
    {
        return $"Name: {Name}\nDiscordID: {DiscordID}\nSteam64ID: {Steam64ID}\nSteamID: {SteamID}\nTeamID: {TeamID}\nKills: {Kills}\nDeaths: {Deaths}";
    }
}
