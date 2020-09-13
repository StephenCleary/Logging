using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Nito.Logging.Internals
{
    /// <summary>
    /// A type that monitors the current <see cref="AppDomain"/> for exceptions and attaches logging scopes to them when thrown.
    /// </summary>
    public sealed class ExceptionLoggingScopesSubscriber : IDisposable
    {
        private readonly EventHandler<FirstChanceExceptionEventArgs> _subscription;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExceptionLoggingScopesSubscriber(LoggingScopes loggingScopes)
        {
            _subscription = (_, args) =>
            {
                var exceptionScopes = args.Exception.TryFindLoggingScopes() ?? ImmutableStack<ILoggingScope>.Empty;
                var currentScopes = loggingScopes.CurrentScopes ?? ImmutableStack<ILoggingScope>.Empty;

                // Combine the exception scopes and current scopes:
                // - Start with the exception scopes.
                // - Add in any current scopes that are not already included in the exception scopes.

                foreach (var scope in currentScopes.Reverse().Where(x => !exceptionScopes.Contains(x)))
                    exceptionScopes = exceptionScopes.Push(scope);

                if (!exceptionScopes.IsEmpty)
                    args.Exception.SetLoggingScopes(exceptionScopes);
            };
            AppDomain.CurrentDomain.FirstChanceException += _subscription;
        }

        /// <summary>
        /// No longer attaches scopes to exceptions.
        /// </summary>
        public void Dispose() => AppDomain.CurrentDomain.FirstChanceException -= _subscription;
    }
}
