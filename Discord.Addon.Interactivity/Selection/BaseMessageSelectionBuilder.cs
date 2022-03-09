using System.Collections.Generic;
using Discord.WebSocket;

namespace Interactivity.Selection
{
    /// <summary>
    /// Represents a base class for creating custom selection builders.
    /// </summary>
    /// <typeparam name="TSelection">The type of the selection to build</typeparam>
    public abstract class BaseMessageSelectionBuilder<TValue>
    {
        /// <summary>
        /// Gets or sets the users who can interact with the selection.
        /// </summary>
        public List<SocketUser> Users { get; set; } = new List<SocketUser>();

        /// <summary>
        /// Gets or sets what the selection should delete.
        /// </summary>
        public DeletionOptions Deletion { get; set; } = DeletionOptions.Invalids;

        /// <summary>
        /// Gets whether the selection is user restricted.
        /// </summary>
        public bool IsUserRestricted => Users.Count > 0;

        /// <summary>
        /// Builds the <see cref="TSelection"/>.
        /// </summary>
        /// <returns></returns>
        public abstract BaseMessageSelection<TValue> Build();
    }
}