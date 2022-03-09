using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Interactivity.Selection
{
    /// <summary>
    /// Represents a base class for creating value selections based on reactions.
    /// </summary>
    /// <typeparam name="TValue">The value type of the Selection</typeparam>
    public abstract class BaseReactionSelection<TValue>
    {
        /// <summary>
        /// Gets which users can interact with the <see cref="BaseReactionSelection{TValue}"/>.
        /// </summary>
        public IReadOnlyCollection<SocketUser> Users { get; }

        /// <summary>
        /// Determites whether everyone can interact with the <see cref="BaseReactionSelection{TValue}"/>.
        /// </summary>
        public bool IsUserRestricted => Users.Count > 0;

        /// <summary>
        /// Gets an ORing determiting what the <see cref="BaseReactionSelection{TValue}"/> will delete.
        /// </summary>
        public DeletionOptions Deletion { get; }

        protected BaseReactionSelection(IReadOnlyCollection<SocketUser> users,
                                        DeletionOptions deletion)
        {
            Users = users;
            Deletion = deletion;
        }

        /// <summary>
        /// Checks whether or not a given <see cref="SocketReaction"/> is a valid input to the Selection.
        /// This is executed for every reaction by a allowed user.
        /// </summary>
        /// <param name="reaction">The reaction to check</param>
        /// <returns></returns>
        protected abstract bool ShouldProcess(SocketReaction reaction);

        /// <summary>
        /// Parses the <see cref="SocketReaction"/> to the related <see cref="TValue"/>.
        /// This is executed after <see cref="ShouldProcess(SocketReaction)"/> returned true.
        /// </summary>
        /// <param name="reaction">The reaction to parse</param>
        /// <returns></returns>
        protected abstract Optional<TValue> ParseValue(SocketReaction reaction);

        /// <summary>
        /// Sends the message that will be used for the selection.
        /// This is only executed if there is no message provided in SendSelectionAsync.
        /// </summary>
        /// <param name="channel">The channel to send the message to</param>
        /// <returns></returns>
        protected abstract Task<IUserMessage> SendMessageAsync(IMessageChannel channel);

        /// <summary>
        /// Modifies the message that will be used for the selection.
        /// This is only executed if there is a message provided in SendSelectionAsync.
        /// </summary>
        /// <param name="message">The message that will be used for the selection</param>
        /// <returns></returns>
        protected virtual Task ModifyMessageAsync(IUserMessage message)
            => Task.CompletedTask;

        /// <summary>
        /// Do something to the message before the actual selection starts.
        /// This is executed after the callback/event got registered.
        /// E.g. adding reactions...
        /// </summary>
        /// <param name="message">The selection message.</param>
        /// <returns></returns>
        protected abstract Task InitializeMessageAsync(IUserMessage message);

        /// <summary>
        /// Modifies the message after the selection has exited.
        /// This should be used to remove reactions, set the content to the result etc.
        /// </summary>
        /// <param name="message">The message that has been used for the selection</param>
        /// <param name="result">The result that the selection captured</param>
        /// <returns></returns>
        protected virtual Task CloseMessageAsync(IUserMessage message, InteractivityResult<TValue> result)
            => Task.CompletedTask;

        internal bool InternalShouldProcess(SocketReaction reaction)
            => (!IsUserRestricted || Users.Any(x => x.Id == reaction.UserId)) && ShouldProcess(reaction);

        internal Optional<TValue> InternalParseValue(SocketReaction reaction)
            => ParseValue(reaction);

        internal Task<IUserMessage> InternalSendMessageAsync(IMessageChannel channel)
            => SendMessageAsync(channel);

        internal Task InternalModifyMessageAsync(IUserMessage message)
            => ModifyMessageAsync(message);

        internal Task InternalInitializeMessageAsync(IUserMessage message)
            => InitializeMessageAsync(message);

        internal Task InternalCloseMessageAsync(IUserMessage message, InteractivityResult<TValue> result)
            => CloseMessageAsync(message, result);
    }
}