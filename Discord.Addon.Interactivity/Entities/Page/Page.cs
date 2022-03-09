using System.Collections.Generic;
using System.Linq;
using Discord;
using Interactivity.Extensions;
using sys = System.Drawing;

namespace Interactivity
{
    /// <summary>
    /// Represents a page
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Gets the text of the <see cref="Page"/>.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the Embed of the <see cref="Page"/>.
        /// </summary>
        public Embed Embed { get; }

        /// <summary>
        /// Creates a new instance of <see cref="PageBuilder"/> with all the values of this <see cref="Page"/>.
        /// </summary>
        /// <returns></returns>
        public PageBuilder ToPageBuilder()
            => new PageBuilder()
            {
                Color = Embed.Color,
                Description = Embed.Description,
                Fields = Embed.Fields.Select(x => x.ToBuilder()).ToList(),
                ImageUrl = Embed.Image?.Url,
                Author = Embed.Author?.ToBuilder(),
                ThumbnailUrl = Embed.Thumbnail?.Url,
                Title = Embed.Title,
                Url = Embed.Url,
                Text = Text
            };

        internal Page(string text = null, sys.Color? color = null,
            string description = null, string title = null, string url = null, string thumbnailUrl = null, string imageUrl = null,
            EmbedAuthorBuilder author = null, List<EmbedFieldBuilder> fields = null, EmbedFooterBuilder footer = null)
        {
            Text = text;

            if (color == null &&
                description == null &&
                title == null &&
                url == null &&
                thumbnailUrl == null &&
                imageUrl == null &&
                fields.Count == 0 &&
                footer == null &&
                author == null)
            {
                Embed = null;
                return;
            }

            Embed = new EmbedBuilder()
            {
                Color = (Color?) color,
                Description = description,
                Title = title,
                Url = url,
                ThumbnailUrl = thumbnailUrl,
                ImageUrl = imageUrl,
                Fields = fields,
                Footer = footer,
                Author = author
            }
            .Build();
        }

        /// <summary>
        /// Creates a new <see cref="Page"/> from an <see cref="Embed"/>.
        /// </summary>
        /// <param name="embed"></param>
        /// <returns></returns>
        public static Page FromEmbed(Embed embed)
            => new Page(color: embed.Color,
                description: embed.Description,
                title: embed.Title,
                url: embed.Url,
                thumbnailUrl: embed.Thumbnail?.Url,
                imageUrl: embed.Image?.Url,
                fields: embed.Fields.Select(x => x.ToBuilder()).ToList());

        /// <summary>
        /// Creates a new <see cref="Page"/> from an <see cref="EmbedBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static Page FromEmbedBuilder(EmbedBuilder builder)
            => new Page(color: builder.Color,
                description: builder.Description,
                title: builder.Title,
                url: builder.Url,
                thumbnailUrl: builder.ThumbnailUrl,
                imageUrl: builder.ImageUrl,
                fields: builder.Fields);
    }
}