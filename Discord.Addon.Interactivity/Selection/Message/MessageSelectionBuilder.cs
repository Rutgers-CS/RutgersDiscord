using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using Interactivity.Extensions;

namespace Interactivity.Selection
{
    public class MessageSelectionBuilder<TValue> : BaseMessageSelectionBuilder<TValue>
    {
        /// <summary>
        /// Gets or sets the items to select from.
        /// </summary>
        public List<TValue> Selectables { get; set; }

        /// <summary>
        /// Gets or sets the function to convert the values into text.
        /// </summary>
        public Func<TValue, string> StringConverter { get; set; } = x => x.ToString();

        /// <summary>
        /// Gets or sets the title of the selection.
        /// </summary>
        public string Title { get; set; } = "Select one of these";

        /// <summary>
        /// Gets or sets whether the selection allows for cancellation.
        /// </summary>
        public bool AllowCancel { get; set; } = true;

        /// <summary>
        /// Gets or sets the message shown for cancel.
        /// Only works if <see cref="AllowCancel"/> is set to <see cref="true"/>.
        /// </summary>
        public string CancelMessage { get; set; } = "Cancel";

        /// <summary>
        /// Gets the <see cref="Page"/> which is sent into the channel.
        /// </summary>
        public PageBuilder SelectionPage { get; set; } = new PageBuilder().WithColor(Color.Blue);

        /// <summary>
        /// Gets the <see cref="Page"/> which will be shown on cancellation.
        /// </summary>
        public PageBuilder CancelledPage { get; set; } = null;

        /// <summary>
        /// Gets the <see cref="Page"/> which will be shown on a timeout.
        /// </summary>
        public PageBuilder TimeoutedPage { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the selectionembed will be added by a default value visualizer.
        /// </summary>
        public bool EnableDefaultSelectionDescription { get; set; } = true;

        /// <summary>
        /// Creates a new <see cref="ReactionSelectionBuilder{TValue}"/> with default values.
        /// </summary>
        public MessageSelectionBuilder() { }

        /// <summary>
        /// Creates a new <see cref="ReactionSelectionBuilder{TValue}"/> with default values.
        /// </summary>
        public static MessageSelectionBuilder<TValue> Default => new MessageSelectionBuilder<TValue>();

        public override BaseMessageSelection<TValue> Build()
        {
            if (Selectables == null)
            {
                throw new ArgumentException(nameof(Selectables));
            }
            if (Selectables.Count == 0)
            {
                throw new InvalidOperationException("You need at least one selectable");
            }

            var selectableDictionary = new Dictionary<string[], TValue>();
            var descriptionBuilder = new StringBuilder();

            for (int i = 0; i < Selectables.Count; i++)
            {
                var selectable = Selectables[i];
                string text = StringConverter.Invoke(selectable);
                selectableDictionary.Add(new string[]
                {
                    $"{i + 1}",
                    $"#{i + 1}",
                    text,
                    $"#{i + 1} - {text}"
                },
                selectable);

                if (EnableDefaultSelectionDescription)
                {
                    descriptionBuilder.AppendLine($"#{i + 1} - {text}");
                }
            }

            if (AllowCancel)
            {
                selectableDictionary.Add(new string[]
                {
                    $"{Selectables.Count + 1}",
                    $"#{Selectables.Count + 1}",
                    CancelMessage,
                    $"#{Selectables.Count + 1} - {CancelMessage}"
                },
                default);

                if (EnableDefaultSelectionDescription)
                {
                    descriptionBuilder.Append($"#{Selectables.Count + 1} - {CancelMessage}");
                }
            }

            if (EnableDefaultSelectionDescription)
            {
                SelectionPage.AddField(Title, descriptionBuilder.ToString());
            }

            return new MessageSelection<TValue>(
                selectableDictionary.AsReadOnly(),
                Users?.AsReadOnly() ?? throw new ArgumentException(nameof(Users)),
                SelectionPage?.Build() ?? throw new ArgumentNullException(nameof(SelectionPage)),
                CancelledPage?.Build(),
                TimeoutedPage?.Build(),
                AllowCancel,
                CancelMessage,
                Deletion
                );
        }

        /// <summary>
        /// Sets the items to select from.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithSelectables(IEnumerable<TValue> selectables)
        {
            Selectables = selectables.ToList();
            return this;
        }

        /// <summary>
        /// Sets the items to select from.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithSelectables(params TValue[] selectables)
        {
            Selectables = selectables.ToList();
            return this;
        }

        /// <summary>
        /// Sets the users who can interact with the selection.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithUsers(IEnumerable<SocketUser> users)
        {
            Users = users.ToList();
            return this;
        }

        /// <summary>
        /// Sets the users who can interact with the selection.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithUsers(params SocketUser[] users)
        {
            Users = users.ToList();
            return this;
        }

        /// <summary>
        /// Sets what the selection should delete.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithDeletion(DeletionOptions deletion)
        {
            Deletion = deletion;
            return this;
        }

        /// <summary>
        /// Sets the selection embed of the selection.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithSelectionEmbed(PageBuilder selectionPage)
        {
            SelectionPage = selectionPage;
            return this;
        }

        /// <summary>
        /// Sets the embed which the selection embed gets modified to after the selection has been cancelled.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithCancelledEmbed(PageBuilder cancelledPage)
        {
            CancelledPage = cancelledPage;
            return this;
        }

        /// <summary>
        /// Sets the embed which the selection embed gets modified to after the selection has timed out.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithTimeoutedEmbed(PageBuilder timeoutedPage)
        {
            TimeoutedPage = timeoutedPage;
            return this;
        }

        /// <summary>
        /// Sets the function to convert the values into possibilites.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithStringConverter(Func<TValue, string> stringConverter)
        {
            StringConverter = stringConverter;
            return this;
        }

        /// <summary>
        /// Sets the message shown for cancel.
        /// Only works if <see cref="AllowCancel"/> is set to <see cref="true"/>.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithCancelDisplayName(string cancelMessage)
        {
            CancelMessage = cancelMessage;
            return this;
        }

        /// <summary>
        /// Sets whether the selection allows for cancellation.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithAllowCancel(bool allowCancel)
        {
            AllowCancel = allowCancel;
            return this;
        }

        /// <summary>
        /// Sets whether the selectionembed will be added by a default value visualizer.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithEnableDefaultSelectionDescription(bool enableDefaultSelectionDescription)
        {
            EnableDefaultSelectionDescription = enableDefaultSelectionDescription;
            return this;
        }

        /// <summary>
        /// Sets the title of the selection.
        /// </summary>
        public MessageSelectionBuilder<TValue> WithTitle(string title)
        {
            Title = title;
            return this;
        }
    }
}