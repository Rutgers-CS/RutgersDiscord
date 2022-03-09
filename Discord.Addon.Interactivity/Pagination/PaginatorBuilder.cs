using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace Interactivity.Pagination
{
    public abstract class PaginatorBuilder
    {
        #region Fields
        /// <summary>
        /// Gets or sets the users who can interact with the <see cref="Paginator"/>.
        /// </summary>
        public List<SocketUser> Users { get; set; } = new List<SocketUser>();

        /// <summary>
        /// Gets or sets the emotes and their related action of the <see cref="Paginator"/>.
        /// </summary>
        public Dictionary<IEmote, PaginatorAction> Emotes { get; set; } = new Dictionary<IEmote, PaginatorAction>();

        /// <summary>
        /// Gets or sets the <see cref="Embed"/> which the <see cref="Paginator"/> gets modified to after cancellation.
        /// </summary>
        public EmbedBuilder CancelledEmbed { get; set; } = new EmbedBuilder().WithColor(Color.Orange).WithTitle("Cancelled! :thumbsup:");

        /// <summary>
        /// Gets or sets the <see cref="Embed"/> which the <see cref="Paginator"/> gets modified to after a timeout.
        /// </summary>
        public EmbedBuilder TimeoutedEmbed { get; set; } = new EmbedBuilder().WithColor(Color.Red).WithTitle("Timed out! :alarm_clock:");

        /// <summary>
        /// Gets or sets what the <see cref="Paginator"/> should delete.
        /// </summary>
        public DeletionOptions Deletion { get; set; } = DeletionOptions.Valid | DeletionOptions.Invalids;

        /// <summary>
        /// Gets or sets the footer in the <see cref="Embed"/> of the <see cref="Paginator"/>.
        /// </summary>
        public PaginatorFooter Footer { get; set; } = PaginatorFooter.PageNumber;

        /// <summary>
        /// Determites whether everyone can interact with the <see cref="Paginator"/>.
        /// </summary>
        public bool IsUserRestricted => Users.Count > 0;
        #endregion

        #region Build
        /// <summary>
        /// Build the <see cref="PaginatorBuilder"/> to a immutable <see cref="Paginator"/>.
        /// </summary>
        /// <returns></returns>
        public abstract Paginator Build(int startPage = 0);
        #endregion

        #region WithValue
        protected PaginatorBuilder WithUsers(params SocketUser[] users)
        {
            Users = users.ToList();
            return this;
        }
        protected PaginatorBuilder WithUsers(IEnumerable<SocketUser> users)
        {
            Users = users.ToList();
            return this;
        }
        protected PaginatorBuilder WithEmotes(Dictionary<IEmote, PaginatorAction> emotes)
        {
            Emotes = emotes;
            return this;
        }
        protected PaginatorBuilder AddEmote(PaginatorAction action, IEmote emote)
        {
            Emotes.Add(emote, action);
            return this;
        }
        protected PaginatorBuilder WithCancelledEmbed(EmbedBuilder embed)
        {
            CancelledEmbed = embed;
            return this;
        }
        protected PaginatorBuilder WithTimoutedEmbed(EmbedBuilder embed)
        {
            TimeoutedEmbed = embed;
            return this;
        }
        protected PaginatorBuilder WithDeletion(DeletionOptions deletion)
        {
            Deletion = deletion;
            return this;
        }
        protected PaginatorBuilder WithFooter(PaginatorFooter footer)
        {
            Footer = footer;
            return this;
        }
        protected PaginatorBuilder WithDefaultEmotes()
        {
            Emotes.Clear();

            Emotes.Add(new Emoji("◀"), PaginatorAction.Backward);
            Emotes.Add(new Emoji("▶"), PaginatorAction.Forward);
            Emotes.Add(new Emoji("⏮"), PaginatorAction.SkipToStart);
            Emotes.Add(new Emoji("⏭"), PaginatorAction.SkipToEnd);
            Emotes.Add(new Emoji("🛑"), PaginatorAction.Exit);

            return this;
        }
        #endregion
    }
}
