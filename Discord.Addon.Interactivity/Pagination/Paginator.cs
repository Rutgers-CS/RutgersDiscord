using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Interactivity.Extensions;

namespace Interactivity.Pagination
{
    /// <summary>
    /// Represents a <see langword="abstract"/> class which allows for multi page messages in discord.
    /// </summary>
    public abstract class Paginator
    {
        #region Fields
        /// <summary>
        /// The index of the current page of the <see cref="Paginator"/>.
        /// </summary>
        public int CurrentPageIndex { get; protected set; }

        /// <summary>
        /// Gets a list of users who can interact with the <see cref="Paginator"/>.
        /// </summary>
        public IReadOnlyCollection<SocketUser> Users { get; }

        /// <summary>
        /// Gets the emotes and their related action of the <see cref="Paginator"/>.
        /// </summary>
        public IReadOnlyDictionary<IEmote, PaginatorAction> Emotes { get; }

        /// <summary>
        /// Gets the <see cref="Embed"/> which the <see cref="Paginator"/> gets modified to after cancellation.
        /// </summary>
        public Embed CancelledEmbed { get; }

        /// <summary>
        /// Gets the <see cref="Embed"/> which the <see cref="Paginator"/> gets modified to after a timeout.
        /// </summary>
        public Embed TimeoutedEmbed { get; }

        /// <summary>
        /// Gets what the <see cref="Paginator"/> should delete.
        /// </summary>
        public DeletionOptions Deletion { get; }

        /// <summary>
        /// Gets the maximum page of the <see cref="Paginator"/>.
        /// </summary>
        public abstract int MaxPageIndex { get; }

        /// <summary>
        /// Determites whether everyone can interact with the <see cref="Paginator"/>.
        /// </summary>
        public bool IsUserRestricted => Users.Count > 0;
        #endregion

        #region Constructor
        public Paginator(IReadOnlyCollection<SocketUser> users, IReadOnlyDictionary<IEmote, PaginatorAction> emotes,
            Embed cancelledEmbed, Embed timeoutedEmbed, DeletionOptions deletion, int startPage)
        {
            Users = users;
            Emotes = emotes;
            CancelledEmbed = cancelledEmbed;
            TimeoutedEmbed = timeoutedEmbed;
            Deletion = deletion;
            CurrentPageIndex = startPage;
        }
        #endregion

        #region Methods
        internal async Task<bool> HandleReactionAsync(BaseSocketClient client, SocketReaction reaction)
        {
            bool valid = await RunChecksAsync(client, reaction).ConfigureAwait(false) && (!IsUserRestricted || Users.Any(x => x.Id == reaction.UserId));

            if (Deletion.HasFlag(DeletionOptions.Invalids) && !valid)
            {
                await client.Rest.RemoveReactionAsync(reaction).ConfigureAwait(false);
            }
            if (Deletion.HasFlag(DeletionOptions.Valid) && valid)
            {
                await client.Rest.RemoveReactionAsync(reaction).ConfigureAwait(false);
            }

            return valid;
        }

        internal Task InitializeMessageAsync(IUserMessage message)
            => message.AddReactionsAsync(Emotes.Keys.ToArray());

        /// <summary>
        /// Sets the <see cref="CurrentPageIndex"/> of the <see cref="Paginator"/>.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public virtual async Task<bool> SetPageAsync(int newPage)
        {
            if (newPage < 0 || CurrentPageIndex == newPage || newPage > MaxPageIndex)
            {
                return false;
            }

            var page = await GetOrLoadPageAsync(newPage).ConfigureAwait(false);

            if (page == null)
            {
                return false;
            }

            CurrentPageIndex = newPage;

            return true;
        }

        /// <summary>
        /// Loads a specific page of the <see cref="Paginator"/>.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public abstract Task<Page> GetOrLoadPageAsync(int page);

        /// <summary>
        /// Loads the current page of the <see cref="Paginator"/>.
        /// </summary>
        /// <returns></returns>
        public Task<Page> GetOrLoadCurrentPageAsync()
            => GetOrLoadPageAsync(CurrentPageIndex);

        /// <summary>
        /// Applies a <see cref="PaginatorAction"/> to the <see cref="Paginator"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual Task<bool> ApplyActionAsync(PaginatorAction action)
        {
            switch (action)
            {
                case PaginatorAction.Backward:
                    return SetPageAsync(CurrentPageIndex - 1);
                case PaginatorAction.Forward:
                    return SetPageAsync(CurrentPageIndex + 1); ;
                case PaginatorAction.SkipToStart:
                    return SetPageAsync(0);
                case PaginatorAction.SkipToEnd:
                    return SetPageAsync(MaxPageIndex);
                default:
                    return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Runs some checks on the <see cref="SocketReaction"/> to make sure it's working with the <see cref="Paginator"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reaction"></param>
        /// <returns></returns>
        public virtual Task<bool> RunChecksAsync(BaseSocketClient client, SocketReaction reaction)
                => Task.FromResult(true);
        #endregion
    }
}
