using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Nito.Disposables;

namespace Nito.Logging.ExceptionContext.Internals
{
    /// <summary>
    /// A logging provider that just tracks scopes.
    /// </summary>
    public sealed class ScopeTrackingLoggerProvider : ILoggerProvider
    {
        private readonly AsyncLocal<ImmutableStack<Scope>> _capturedScopes = new AsyncLocal<ImmutableStack<Scope>>();
        private readonly Logger _logger;
        private readonly EventHandler<FirstChanceExceptionEventArgs> _subscription;

        /// <summary>
        /// Creates the logging provider.
        /// </summary>
        public ScopeTrackingLoggerProvider()
        {
            _logger = new Logger(this);
            _subscription = (_, args) => args.Exception.SetScopes(CurrentScopes);
            AppDomain.CurrentDomain.FirstChanceException += _subscription;
        }

        /// <summary>
        /// Gets the current stack of scopes.
        /// </summary>
        public ImmutableStack<Scope> CurrentScopes => _capturedScopes.Value ?? ImmutableStack<Scope>.Empty;

        /// <summary>
        /// No longer attaches scopes to exceptions.
        /// </summary>
        public void Dispose() => AppDomain.CurrentDomain.FirstChanceException -= _subscription;

        ILogger ILoggerProvider.CreateLogger(string categoryName) => _logger;

        private IDisposable BeginScope<TState>(TState state)
        {
            var originalCapturedScopesValue = _capturedScopes.Value;
            var scopeStack = originalCapturedScopesValue ?? ImmutableStack<Scope>.Empty;
            scopeStack = scopeStack.Push(new Scope<TState>(state));
            _capturedScopes.Value = scopeStack;
            return new AnonymousDisposable(() => _capturedScopes.Value = originalCapturedScopesValue!);
        }

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

            public IDisposable BeginScope<TState>(TState state) => _provider.BeginScope(state);
        }
    }
}
