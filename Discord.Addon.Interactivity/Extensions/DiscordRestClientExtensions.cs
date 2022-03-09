using Discord.Rest;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Interactivity.Extensions
{
    internal static partial class Extensions
    {
        public static Task RemoveReactionAsync(this DiscordRestClient client, SocketReaction reaction)
            => client.RemoveReactionAsync(reaction.Channel.Id, reaction.MessageId, reaction.UserId, reaction.Emote);
    }
}
