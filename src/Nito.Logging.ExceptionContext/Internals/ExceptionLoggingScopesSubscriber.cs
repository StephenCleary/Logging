using System;
using System.Collections.Generic;
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
            _subscription = (_, args) => args.Exception.SetLoggingScopes(loggingScopes.CurrentScopes);
            AppDomain.CurrentDomain.FirstChanceException += _subscription;
        }

        /// <summary>
        /// No longer attaches scopes to exceptions.
        /// </summary>
        public void Dispose() => AppDomain.CurrentDomain.FirstChanceException -= _subscription;
    }
}
