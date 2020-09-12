using System.Collections.Generic;
using System.Linq;

namespace UnitTests.Utility
{
    /// <summary>
    /// Contains the logic used to extract scope information used in Microsoft.Extensions.Logging.
    /// See https://nblumhardt.com/2016/11/ilogger-beginscope/ for details.
    /// </summary>
    public static class ScopeUtility
    {
        /// <summary>
        /// Retrieves structured data from a scope object. Never returns <c>null</c>.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, object>> GetStructuredData<T>(T state)
        {
            if (state is IEnumerable<KeyValuePair<string, object>> data)
                return data.Where(x => x.Key != OriginalFormat);
            return Enumerable.Empty<KeyValuePair<string, object>>();
        }

        /// <summary>
        /// The special key used by Microsoft's logging extensions to hold the original format string.
        /// </summary>
        private const string OriginalFormat = "{OriginalFormat}";
    }
}
