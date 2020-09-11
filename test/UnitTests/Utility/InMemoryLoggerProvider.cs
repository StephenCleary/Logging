using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;
using Nito.LogExceptionContext.Internals;

namespace UnitTests.Utility
{
    public sealed class InMemoryLoggerProvider : ILoggerProvider
    {
        private readonly object _mutex = new object();

        public void Dispose() { }

        public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);

        public ImmutableList<LogMessage> Messages { get; private set; } = ImmutableList<LogMessage>.Empty;

        public sealed class LogMessage
        {
            public string CategoryName { get; set; }
            public LogLevel LogLevel { get; set; }
            public EventId EventId { get; set; }
            public Exception Exception { get; set; }
            public string Message { get; set; }
            public IReadOnlyList<string> ScopeStrings { get; set; }
            public IReadOnlyDictionary<string, object> ScopeValues { get; set; }
        }

        private void AddMessage(LogMessage message)
        {
            lock (_mutex)
                Messages = Messages.Add(message);
        }

        private sealed class Logger : ILogger
        {
            private readonly InMemoryLoggerProvider _provider;
            private readonly string _categoryName;

            public Logger(InMemoryLoggerProvider provider, string categoryName)
            {
                _provider = provider;
                _categoryName = categoryName;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var messageScopeData = ScopeUtility.GetStructuredData(state);
                var messageScopeString = ScopeUtility.TryGetStringRepresentation(state);

                // TODO: read from captured scopes instead of starting with empty collections.
                var scopeStrings = new List<string>();
                if (messageScopeString != null)
                    scopeStrings.Add(messageScopeString);
                var scopeValues = new Dictionary<string, object>();
                foreach (var (key, value) in messageScopeData)
                    scopeValues[key] = value;

                _provider.AddMessage(new LogMessage
                {
                    CategoryName = _categoryName,
                    LogLevel = logLevel,
                    EventId = eventId,
                    Exception = exception,
                    Message = formatter(state, exception),
                    ScopeStrings = scopeStrings,
                    ScopeValues = scopeValues,
                });
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state)
            {
                // TODO: capture scopes.
                throw new NotImplementedException();
            }
        }
    }
}
