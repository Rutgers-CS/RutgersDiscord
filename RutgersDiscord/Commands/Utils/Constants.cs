using System.Collections.Generic;

public static class Constants
{
    public const ulong guild = 670683408057237547;
    public static class Database
    {
        public const string matchTable = "match_list";
        public const string playerTable = "player_list";
        public const string teamTable = "team_list";
    }
    public static class Role
    {
        public const ulong everyone = 670683408057237547;
        public const ulong admin = 670684819306577921;
    }

    public static class ChannelCategories
    {
        public const ulong matches = 952771705485553744;
    }

    public static readonly Dictionary<string, string> ImgurAlbum = new()
    {
        { "Pithead", "https://i.imgur.com/5dZFoBw.jpg" },
        { "Guard", "https://i.imgur.com/fN5Hw32.jpg" },
        { "Sunburn", "https://i.imgur.com/CkuPfDZ.jpg" },
        { "Hive", "https://i.imgur.com/BE72y3M.jpg" },
        { "Grassetto", "https://i.imgur.com/NLHULQj.jpg" },
        { "Elysion", "https://i.imgur.com/HjuguVF.jpg" },
        { "Crete", "https://i.imgur.com/cK9Icys.jpg" }
    };
}
