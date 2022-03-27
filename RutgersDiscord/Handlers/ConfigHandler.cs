using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutgersDiscord.Handlers
{
    public class ConfigHandler
    {
        public Settings settings;

        public ConfigHandler()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("./config.json", false, false)
                .Build();

            settings = configuration.GetRequiredSection("Settings").Get<Settings>();
        }
    }

    public class Settings
    {
        public Application ApplicationSettings { get; set; }
        public Discord DiscordSettings { get; set; }
        public DatHost DatHostSettings { get; set; }
    }

    public class Application
    {
        public string PublicIP { get; set; }
        public int Port { get; set; }
        public string SteamWebAPIToken { get; set; }
    }

    public class Discord
    {
        public string BotToken { get; set; }
        public ulong Guild { get; set; }
        public Channel Channels { get; set; }
        public ChannelCategory ChannelCategories { get; set; }
        public Role Roles { get; set; }
        public User Users { get; set; }
    }

    public class Channel
    {
        public ulong Announcements { get; set; }
        public ulong SCGeneral { get; set; }
        public ulong SCAnnouncements { get; set; }
        public ulong SCAdmin { get; set; }
    }

    public class ChannelCategory
    {
        public ulong MatchesCategory { get; set; }
    }

    public class Role
    {
        public ulong Everyone { get; set; }
        public ulong Admin { get; set; }
        public ulong ScarletClassic { get; set; }
        public ulong Goodfellas { get; set; }
    }

    public class User
    {
        public ulong Galifi { get; set; }
        public ulong Op7day { get; set; }
        public ulong Guihori { get; set; }
    }

    public class DatHost
    {
        public string DatHostEmail { get; set; }
        public string DatHostPassword { get; set; }
        public string TemplateServerID { get; set; }
    }
}
