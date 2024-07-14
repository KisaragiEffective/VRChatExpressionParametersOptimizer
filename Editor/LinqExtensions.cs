using System.Collections.Generic;
using System.Linq;

namespace VRChatExpressionParametersOptimizer.Editor
{
    internal static class LinqExtensions
    {
        internal static IEnumerable<T> Chain<T>(this IEnumerable<T> self, IEnumerable<T> other) 
            => new[] { self, other }.SelectMany(a => a);
    }
}
