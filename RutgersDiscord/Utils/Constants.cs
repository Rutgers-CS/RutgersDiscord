using Discord;
using System;
using System.Collections.Generic;

public static class Constants
{
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
