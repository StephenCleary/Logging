using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Nito.Logging.Internals
{
    /// <summary>
    /// A logging provider that just tracks logging scopes.
    /// </summary>
    public sealed class LoggingScopeTrackingLoggerProvider : ILoggerProvider
    {
        private readonly LoggingScopes _loggingScopes;
        private readonly Logger _logger;

        /// <summary>
        /// Creates the logging provider.
        /// </summary>
        public LoggingScopeTrackingLoggerProvider(LoggingScopes loggingScopes)
        {
            _loggingScopes = loggingScopes;
            _logger = new Logger(this);
        }

        /// <summary>
        /// Noop.
        /// </summary>
        public void Dispose() { }

        ILogger ILoggerProvider.CreateLogger(string categoryName) => _logger;

        private sealed class Logger : ILogger
        {
            private readonly LoggingScopeTrackingLoggerProvider _provider;

            public Logger(LoggingScopeTrackingLoggerProvider provider)
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
