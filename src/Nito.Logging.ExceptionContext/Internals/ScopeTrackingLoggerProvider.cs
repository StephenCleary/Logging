using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Nito.Logging.Internals
{
    /// <summary>
    /// A logging provider that just tracks scopes.
    /// </summary>
    public sealed class ScopeTrackingLoggerProvider : ILoggerProvider
    {
        private readonly LoggingScopes _loggingScopes = new LoggingScopes();
        private readonly Logger _logger;
        private readonly EventHandler<FirstChanceExceptionEventArgs> _subscription;

        /// <summary>
        /// Creates the logging provider.
        /// </summary>
        public ScopeTrackingLoggerProvider()
        {
            _logger = new Logger(this);
            _subscription = (_, args) => args.Exception.SetScopes(_loggingScopes.CurrentScopes);
            AppDomain.CurrentDomain.FirstChanceException += _subscription;
        }

        /// <summary>
        /// No longer attaches scopes to exceptions.
        /// </summary>
        public void Dispose() => AppDomain.CurrentDomain.FirstChanceException -= _subscription;

        ILogger ILoggerProvider.CreateLogger(string categoryName) => _logger;

        private sealed class Logger : ILogger
        {
            private readonly ScopeTrackingLoggerProvider _provider;

            public Logger(ScopeTrackingLoggerProvider provider)
            {
                _provider = provider;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
            }

            public bool IsEnabled(LogLevel logLevel) => false;

            public IDisposable BeginScope<TState>(TState state) => _provider._loggingScopes.PushLoggingScope(state);
        }
    }
}
