using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Interactivity.Extensions
{
    internal static partial class Extensions
    {
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
            => new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
}