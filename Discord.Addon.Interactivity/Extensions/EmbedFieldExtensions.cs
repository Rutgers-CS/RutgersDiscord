using Discord;

namespace Interactivity.Extensions
{
    internal static partial class Extensions
    {
        public static EmbedFieldBuilder ToBuilder(this EmbedField field)
            => new EmbedFieldBuilder()
            {
                IsInline = field.Inline,
                Name = field.Name,
                Value = field.Value
            };
    }
}
