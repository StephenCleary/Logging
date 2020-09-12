using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Nito.Logging.Internals
{
    /// <summary>
    /// A captured scope.
    /// </summary>
    public interface IScope
    {
        /// <summary>
        /// Applies this scope to the logger.
        /// </summary>
        IDisposable? Begin(ILogger logger);
    }

    /// <summary>
    /// A captured scope.
    /// </summary>
    public sealed class Scope<T> : IScope
    {
        /// <summary>
        /// Creates a scope.
        /// </summary>
        public Scope(T value)
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
