internal class PlayerInfo
{
    //Used to parse database values
    public string Name { get; set; }
    public long SteamID { get; set; }
    public long DiscordID { get; set; }
    public string TeamName { get; set; }
    public byte MatchesWon { get; set; }
    public byte MatchesLost { get; set; }

    //Potentially extra features for leaderboard
    public ushort RoundsWon { get; set; }
    public ushort RoundsLost { get; set; }
    //Calculate KD from these values rather than storing it in database
    public ushort Kills { get; set; }
    public ushort Deaths { get; set; }
    
}
