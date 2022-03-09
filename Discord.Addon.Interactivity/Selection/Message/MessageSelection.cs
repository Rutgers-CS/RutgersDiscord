using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Interactivity.Selection
{
    /// <summary>
    /// Represents the default implementation of <see cref="BaseMessageSelection{TValue}"/> which comes with a lot of options suitable for most users.
    /// This class is immutable!
    /// </summary>
    /// <typeparam name="TValue">The type of the values to select from</typeparam>
    public class MessageSelection<TValue> : BaseMessageSelection<TValue>
    {
        /// <summary>
        /// Gets the items to select from.
        /// </summary>
        public IReadOnlyDictionary<string[], TValue> Selectables { get; }

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
        /// Gets the message shown for cancel.
        /// Only works if <see cref="AllowCancel"/> is set to <see cref="true"/>.
        /// </summary>
        public string CancelMessage { get; }

        public MessageSelection(IReadOnlyDictionary<string[], TValue> selectables, IReadOnlyCollection<SocketUser> users,
            Page selectionPage, Page cancelledPage, Page timeoutedPage,
            bool allowCancel, string cancelMessage,
            DeletionOptions deletion)
            : base(users, deletion)
        {
            Selectables = selectables;
            SelectionPage = selectionPage;
            CancelledPage = cancelledPage;
            TimeoutedPage = timeoutedPage;
            AllowCancel = allowCancel;
            CancelMessage = cancelMessage;
        }

        protected override bool ShouldProcess(SocketMessage message)
            => message.Source == MessageSource.User && Selectables.Any(x => x.Key.Contains(message.Content));

        protected override Optional<TValue> ParseValue(SocketMessage message)
        {
            var result = Selectables.First(x => x.Key.Contains(message.Content));

            return result.Equals(Selectables.Last())
                ? Optional<TValue>.Unspecified
                : result.Value;
        }

        protected override Task<IUserMessage> SendMessageAsync(IMessageChannel channel)
            => channel.SendMessageAsync(text: SelectionPage.Text, embed: SelectionPage.Embed);

        protected override Task ModifyMessageAsync(IUserMessage message)
            => message.ModifyAsync(x => { x.Content = SelectionPage.Text; x.Embed = SelectionPage.Embed; });

        protected override async Task CloseMessageAsync(IUserMessage message, InteractivityResult<TValue> result)
        {
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