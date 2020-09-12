using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Nito.Logging.Internals
{
    /// <summary>
    /// A logging provider wrapper that applies scopes attached to exceptions.
    /// This does not work with .NET Core's Dependency Injection (due to limitations of the .NET Core DI and how the options pattern supersedes it), but could be useful with other DI providers.
    /// </summary>
    public sealed class ScopeApplyingLoggingProviderWrapper : ILoggerProvider
    {
        private readonly ILoggerProvider _innerProvider;

        /// <summary>
        /// Creates a provider wrapping the specified provider.
        /// </summary>
        public ScopeApplyingLoggingProviderWrapper(ILoggerProvider innerProvider)
        {
            _innerProvider = innerProvider;
        }

        ILogger ILoggerProvider.CreateLogger(string categoryName) => new Logger(_innerProvider.CreateLogger(categoryName));

        /// <summary>
        /// Disposes the wrapped provider.
        /// </summary>
        public void Dispose() => _innerProvider.Dispose();

        private sealed class Logger : ILogger
        {
            private readonly ILogger _innerLogger;

            public Logger(ILogger innerLogger)
            {
                _innerLogger = innerLogger;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                using var _ = _innerLogger.BeginCapturedExceptionScopes(exception);
                _innerLogger.Log(logLevel, eventId, state, exception, formatter);
            }

            public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

            public IDisposable BeginScope<TState>(TState state) => _innerLogger.BeginScope(state);
        }
    }
}
