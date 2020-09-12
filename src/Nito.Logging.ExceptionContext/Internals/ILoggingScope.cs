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

    /// <summary>
    /// A captured scope.
    /// </summary>
    public sealed class LoggingScope<T> : ILoggingScope
    {
        /// <summary>
        /// Creates a scope.
        /// </summary>
        public LoggingScope(T value)
        {
            Value = value;
        }

        /// <summary>
        /// The scope value.
        /// </summary>
        public T Value { get; }

        /// <inheritdoc />
        public IDisposable? Begin(ILogger logger) => logger?.BeginScope(Value);
    }
}
