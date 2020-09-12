using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Nito.Disposables;

namespace Nito.Logging.ExceptionContext.Internals
{
    /// <summary>
    /// A logging provider wrapper that applies scopes attached to exceptions.
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
                var scopes = exception?.TryGetScopes();
                using var disposable = new CollectionDisposable();
                if (scopes != null)
                {
                    foreach (var scope in scopes)
                    {
                        var innerScope = scope.Begin(_innerLogger);
                        if (innerScope != null)
                            disposable.Add(innerScope);
                    }
                }

                _innerLogger.Log(logLevel, eventId, state, exception, formatter);
            }

            public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

            public IDisposable BeginScope<TState>(TState state) => _innerLogger.BeginScope(state);
        }
    }
}
