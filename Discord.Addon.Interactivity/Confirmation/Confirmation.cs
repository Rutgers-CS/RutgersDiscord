using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Interactivity.Confirmation
{
    /// <summary>
    /// Used to confirm an action via reaction on a <see cref="IUserMessage"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Confirmation
    {
        /// <summary>
        /// Gets the content to be displayed in the <see cref="Confirmation"/>
        /// </summary>
        public Page Content { get; }

        /// <summary>
        /// Gets the users allowed to interact with the <see cref="Confirmation"/>.
        /// </summary>
        public IReadOnlyCollection<SocketUser> Users { get; }

        /// <summary>
        /// Gets the <see cref="IEmote"/> to confirm the <see cref="Confirmation"/>.
        /// </summary>
        public IEmote ConfirmEmote { get; }

        /// <summary>
        /// Gets the <see cref="IEmote"/> to decline the <see cref="Confirmation"/>.
        /// </summary>
        public IEmote DeclineEmote { get; }

        /// <summary>
        /// Gets the <see cref="Embed"/> the <see cref="Confirmation"/> gets modified to on timeout.
        /// </summary>
        public Embed TimeoutedEmbed { get; }

        /// <summary>
        /// Gets the <see cref="Embed"/> the <see cref="Confirmation"/> gets modified to on cancel.
        /// </summary>
        public Embed CancelledEmbed { get; }

        /// <summary>
        /// Gets what the <see cref="Confirmation"/> should delete.
        /// </summary>
        public DeletionOptions Deletion { get; }

        internal IEmote[] Emotes => new IEmote[] { ConfirmEmote, DeclineEmote };

        internal Confirmation(Page content, IReadOnlyCollection<SocketUser> users, IEmote confirmEmote, IEmote declineEmote, Embed timeoutedEmbed, Embed cancelledEmbed, DeletionOptions deletion)
        {
            Content = content;
            Users = users;
            ConfirmEmote = confirmEmote;
            DeclineEmote = declineEmote;
            TimeoutedEmbed = timeoutedEmbed;
            CancelledEmbed = cancelledEmbed;
            Deletion = deletion;
        }

        internal Predicate<SocketReaction> GetFilter()
            => reaction
            => Emotes.Contains(reaction.Emote) && (!Users.Any() || Users.Where(x => x.Id == reaction.UserId).Any());

        internal Func<SocketReaction, bool, Task> GetActions()
            => async (reaction, valid) =>
        {
            if (Deletion.HasFlag(DeletionOptions.Invalids) && !valid)
            {
                await reaction.Message.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            }
        };
    }
}
