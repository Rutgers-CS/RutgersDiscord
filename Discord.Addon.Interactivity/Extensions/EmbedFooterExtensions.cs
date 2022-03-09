using Discord;

namespace Interactivity.Extensions
{
    internal static partial class Extensions
    {
        public static EmbedFooterBuilder ToBuilder(this EmbedFooter footer)
            => new EmbedFooterBuilder()
            {
                Text = footer.Text,
                IconUrl = footer.IconUrl,
            };
    }
}