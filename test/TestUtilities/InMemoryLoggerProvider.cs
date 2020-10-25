using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;
using Nito.Disposables;

namespace TestUtilities
{
    [ExcludeFromCodeCoverage]
    public sealed class InMemoryLoggerProvider : ILoggerProvider
    {
        private readonly object _mutex = new object();
        private readonly AsyncLocal<ImmutableStack<ImmutableDictionary<string, object>>> _scopeStack = new AsyncLocal<ImmutableStack<ImmutableDictionary<string, object>>>();

        public void Dispose() { }

        public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);

        public ImmutableList<LogMessage> Messages { get; private set; } = ImmutableList<LogMessage>.Empty;

        [ExcludeFromCodeCoverage]
        public sealed class LogMessage
        {
            public string CategoryName { get; set; }
            public LogLevel LogLevel { get; set; }
            public EventId EventId { get; set; }
            public Exception Exception { get; set; }
            public string Message { get; set; }
            public IReadOnlyDictionary<string, object> ScopeValues { get; set; }
        }

        private void AddMessage(LogMessage message)
        {
            lock (_mutex)
                Messages = Messages.Add(message);
        }

        private IDisposable PushScope(IEnumerable<KeyValuePair<string, object>> scopeData)
        {
            var originalScope = _scopeStack.Value;
            var currentScope = originalScope ?? ImmutableStack<ImmutableDictionary<string, object>>.Empty;
            var currentScopeData = currentScope.IsEmpty ? ImmutableDictionary<string, object>.Empty : currentScope.Peek();
            currentScopeData = currentScopeData.SetItems(scopeData);
            currentScope = currentScope.Push(currentScopeData);
            _scopeStack.Value = currentScope;
            return new AnonymousDisposable(() => _scopeStack.Value = originalScope);
        }

        private ImmutableDictionary<string, object> CurrentScopeData
        {
            get
            {
                var currentScope = _scopeStack.Value ?? ImmutableStack<ImmutableDictionary<string, object>>.Empty;
                return currentScope.IsEmpty ? ImmutableDictionary<string, object>.Empty : currentScope.Peek();
            }
        }

        [ExcludeFromCodeCoverage]
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
                var scopeData = _provider.CurrentScopeData;
                scopeData = scopeData.SetItems(ScopeUtility.GetStructuredData(state));

                _provider.AddMessage(new LogMessage
                {
                    CategoryName = _categoryName,
                    LogLevel = logLevel,
                    EventId = eventId,
                    Exception = exception,
                    Message = formatter(state, exception),
                    ScopeValues = scopeData,
                });
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => _provider.PushScope(ScopeUtility.GetStructuredData(state));
        }
    }
}
