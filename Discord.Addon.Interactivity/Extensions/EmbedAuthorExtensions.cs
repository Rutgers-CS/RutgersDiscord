using Discord;

namespace Interactivity.Extensions
{
    internal static partial class Extensions
    {
        public static EmbedAuthorBuilder ToBuilder(this EmbedAuthor author)
            => new EmbedAuthorBuilder()
            {
                Name = author.Name,
                Url = author.Url,
                IconUrl = author.IconUrl
            };
    }
}