using Discord;
using Discord.WebSocket;
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

        private static DiscordSocketClient _client;
        public ConfigHandler(DiscordSocketClient client)
        {
            _client = client;

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("./config.json", false, false)
                .Build();

            settings = configuration.GetRequiredSection("Settings").Get<Settings>();
        }

        //actionDescription: What was issued
        public async Task LogAsync(string actionDescription, string message, ulong user = 0, ulong channel = 0)
        {
            SocketTextChannel logChannel = _client.GetGuild(settings.DiscordSettings.Guild).GetTextChannel(settings.DiscordSettings.Channels.Logs);
            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(actionDescription)
                .WithDescription(message)
                .WithTimestamp(DateTimeOffset.Now);

            if(user != 0)
            {
                SocketUser u = _client.GetUser(user);
                embed.WithAuthor(new EmbedAuthorBuilder()
                    .WithName(u.Username)
                    .WithIconUrl(u.GetDefaultAvatarUrl()));
            }
            if(channel != 0)
            {
                SocketTextChannel c = _client.GetGuild(settings.DiscordSettings.Guild).GetTextChannel(channel);
                embed.WithFields(new EmbedFieldBuilder {Name = c.Name,Value = c.Mention });
            }

            await logChannel.SendMessageAsync(embed: embed.Build());
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
        public ulong Logs { get; set; }
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
