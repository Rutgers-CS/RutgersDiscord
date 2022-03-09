using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Interactivity.Pagination
{
    public sealed class LazyPaginator : Paginator
    {
        private ConcurrentDictionary<int, Page> CachedPages { get; }

        /// <summary>
        /// Gets the method used to load the pages of the <see cref="Paginator"/> lazily.
        /// </summary>
        public Func<int, Task<Page>> PageFactory { get; }

        /// <summary>
        /// Gets the maximum page of the <see cref="Paginator"/>.
        /// </summary>
        public override int MaxPageIndex { get; }

        internal LazyPaginator(IReadOnlyCollection<SocketUser> users, IReadOnlyDictionary<IEmote, PaginatorAction> emotes,
            Embed cancelledEmbed, Embed timeoutedEmbed, DeletionOptions deletion,
            Func<int, Task<Page>> pageFactory, int startPage, int maxPageIndex)
            : base(users, emotes, cancelledEmbed, timeoutedEmbed, deletion, startPage)
        {
            CachedPages = new ConcurrentDictionary<int, Page>();
            PageFactory = pageFactory;
            MaxPageIndex = maxPageIndex;
        }

        public override async Task<Page> GetOrLoadPageAsync(int pageNumber)
        {
            if (!CachedPages.TryGetValue(pageNumber, out var page))
            {
                page = await PageFactory(pageNumber).ConfigureAwait(false);
                CachedPages.TryAdd(pageNumber, page);
            }

            return page;
        }

        public override Task<bool> RunChecksAsync(BaseSocketClient client, SocketReaction reaction)
            => Task.FromResult(Emotes.Keys.Any(x => x.Equals(reaction.Emote)));
    }
}