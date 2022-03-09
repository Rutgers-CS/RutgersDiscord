using System;

namespace Interactivity
{
    /// <summary>
    /// The result of interactivityrequests.
    /// </summary>
    /// <typeparam name="T">The type of the value in this <see cref="InteractivityResult{T}"/>.</typeparam>
    public sealed class InteractivityResult<T>
    {
        /// <summary>
        /// The <see cref="T"/> representing the result returned by the interactivity action.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// The time passed between starting the interactivity action and getting its result.
        /// </summary>
        public TimeSpan Elapsed { get; }

        /// <summary>
        /// Wether or not the interactivity action timed out.
        /// </summary>
        public bool IsTimeouted { get; }

        /// <summary>
        /// Wether or not the interactivity action was cancelled by the user or by a cancellation token.
        /// </summary>
        public bool IsCancelled { get; }

        /// <summary>
        /// Whether or not the interactivity action was successful and returned a Value <see cref="T"/>.
        /// </summary>
        public bool IsSuccess => !IsCancelled && !IsTimeouted;

        internal InteractivityResult(T value, TimeSpan elapsed, bool isTimeouted, bool isCancelled)
        {
            Value = value;
            Elapsed = elapsed;
            IsTimeouted = isTimeouted;
            IsCancelled = isCancelled;
        }
    }
}