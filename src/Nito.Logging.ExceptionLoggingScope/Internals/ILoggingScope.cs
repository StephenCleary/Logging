using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Nito.Logging.Internals
{
    /// <summary>
    /// A captured logging scope.
    /// </summary>
    public interface ILoggingScope
    {
        /// <summary>
        /// Applies this scope to the logger.
        /// </summary>
        IDisposable? Begin(ILogger logger);
    }
}
