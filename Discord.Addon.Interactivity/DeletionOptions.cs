using System;

namespace Interactivity
{
    [Flags]
    public enum DeletionOptions
    {
        /// <summary>
        /// Dont delete anything
        /// </summary>
        None = 0,
        /// <summary>
        /// Whether to delete the valid response.
        /// </summary>
        Valid = 1,
        /// <summary>
        /// Whether to delete invalids responses.
        /// </summary>
        Invalids = 2,
        /// <summary>
        /// Whether to delete the selection after it captured a <see cref="InteractivityResult{T}"/>.
        /// </summary>
        AfterCapturedContext = 4
    }
}
