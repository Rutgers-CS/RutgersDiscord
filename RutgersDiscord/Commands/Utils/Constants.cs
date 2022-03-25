using Discord;
using System;
using System.Collections.Generic;

public static class Constants
{
    public const ulong guild = 670683408057237547; //CSGOID 562432001160904734
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
        public const ulong scarletClassic = 955316333560602624; //CSGOID 955314343296262154
    }

    public static class ChannelCategories
    {
        public const ulong matches = 952771705485553744;
    }

    public static class Channels
    {
        public const ulong scGeneral = 670683408057237550; //CSGOID 955315101022425148
    }

    public static class EmbedColors
    {
        public static readonly Color active = new Color(250, 218, 94);
        public static readonly Color reject = new Color(25, 25, 25);
        public static readonly Color accept = new Color(20, 184, 37);
    }

    [Obsolete("Switch to database for image links.")]
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
