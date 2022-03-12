using Dapper.Contrib.Extensions;
[Table ("player_list")]
public class PlayerInfo
{
    [Key]
    public long DiscordID { get; set; }
    public long? SteamID { get; set; }
    public long? TeamID { get; set; }
}
