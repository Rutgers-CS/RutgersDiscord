using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Interactivity.Selection
{
    /// <summary>
    /// Represents the default implementation of <see cref="BaseReactionSelection{TValue}"/> which comes with a lot of options suitable for most users.
    /// This class is immutable!
    /// </summary>
    /// <typeparam name="TValue">The type of the values to select from</typeparam>
    public sealed class ReactionSelection<TValue> : BaseReactionSelection<TValue>
    {
        /// <summary>
        /// Gets the items to select from.
        /// </summary>
        public IReadOnlyDictionary<IEmote, TValue> Selectables { get; }

        /// <summary>
        /// Gets the <see cref="Page"/> which is sent into the channel.
        /// </summary>
        public Page SelectionPage { get; }

        /// <summary>
        /// Gets the <see cref="Page"/> which will be shown on cancellation.
        /// </summary>
        public Page CancelledPage { get; }

        /// <summary>
        /// Gets the <see cref="Page"/> which will be shown on a timeout.
        /// </summary>
        public Page TimeoutedPage { get; }

        /// <summary>
        /// Gets whether the selection allows for cancellation.
        /// </summary>
        public bool AllowCancel { get; }

        /// <summary>
        /// Gets the emote used for cancelling.
        /// Only works if <see cref="AllowCancel"/> is set to <see cref="true"/>.
        /// </summary>
        public IEmote CancelEmote { get; }

        public ReactionSelection(IReadOnlyDictionary<IEmote, TValue> selectables, IReadOnlyCollection<SocketUser> users,
            Page selectionPage, Page cancelledPage, Page timeoutedPage,
            bool allowCancel, IEmote cancelEmote,
            DeletionOptions deletion)
            : base(users, deletion)
        {
            Selectables = selectables;
            SelectionPage = selectionPage;
            CancelledPage = cancelledPage;
            TimeoutedPage = timeoutedPage;
            AllowCancel = allowCancel;
            CancelEmote = cancelEmote;
        }

        protected override bool ShouldProcess(SocketReaction reaction)
            => (AllowCancel && reaction.Emote.Equals(CancelEmote)) || Selectables.ContainsKey(reaction.Emote);

        protected override Optional<TValue> ParseValue(SocketReaction reaction)
            => AllowCancel && reaction.Emote.Equals(CancelEmote)
                ? Optional<TValue>.Unspecified
                : Selectables[reaction.Emote];

        protected override Task<IUserMessage> SendMessageAsync(IMessageChannel channel)
            => channel.SendMessageAsync(text: SelectionPage.Text, embed: SelectionPage.Embed);

        protected override Task ModifyMessageAsync(IUserMessage message)
            => message.ModifyAsync(x => { x.Content = SelectionPage.Text; x.Embed = SelectionPage.Embed; });

        protected override async Task InitializeMessageAsync(IUserMessage message)
        {
            await message.AddReactionsAsync(Selectables.Keys.ToArray());
            if (AllowCancel)
            {
                await message.AddReactionAsync(CancelEmote);
            }
        }

        protected override async Task CloseMessageAsync(IUserMessage message, InteractivityResult<TValue> result)
        {
            //the remove all reactions endpoint requires the user to have the manage messages permission
            //the user wont have this in a DM channel and calling this will throw HttpExceptions
            if (message.Channel is not IDMChannel)
            {
                await message.RemoveAllReactionsAsync();
            }
            if (result.IsCancelled && CancelledPage != null)
            {
                await message.ModifyAsync(x => { x.Content = CancelledPage.Text; x.Embed = CancelledPage.Embed; });
            }
            if (result.IsTimeouted && TimeoutedPage != null)
            {
                await message.ModifyAsync(x => { x.Content = TimeoutedPage.Text; x.Embed = TimeoutedPage.Embed; });
            }
        }
    }
}