using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Nito.Logging.ExceptionContext.Internals
{
    /// <summary>
    /// A captured scope.
    /// </summary>
    public abstract class Scope
    {
        /// <summary>
        /// Applies this scope to the logger.
        /// </summary>
        public abstract IDisposable Begin(ILogger logger);
    }

    /// <summary>
    /// A captured scope.
    /// </summary>
    public sealed class Scope<T> : Scope
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
        public override IDisposable Begin(ILogger logger) => logger.BeginScope(Value);
    }
}
