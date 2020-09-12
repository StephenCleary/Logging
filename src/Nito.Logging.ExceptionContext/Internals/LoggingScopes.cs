using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Nito.Disposables;

namespace Nito.Logging.Internals
{
    /// <summary>
    /// A stack of logging scopes.
    /// </summary>
    public sealed class LoggingScopes
    {
        private readonly AsyncLocal<ImmutableStack<ILoggingScope>> _capturedScopes = new AsyncLocal<ImmutableStack<ILoggingScope>>();

        /// <summary>
        /// Gets the current stack of scopes.
        /// </summary>
        public ImmutableStack<ILoggingScope> CurrentScopes => _capturedScopes.Value ?? ImmutableStack<ILoggingScope>.Empty;

        /// <summary>
        /// Pushes a logging scope onto the stack. Returns a disposable that pops this logging scope when disposed.
        /// </summary>
        public IDisposable PushLoggingScope<TState>(TState state)
        {
            var originalCapturedScopesValue = _capturedScopes.Value;
            var scopeStack = originalCapturedScopesValue ?? ImmutableStack<ILoggingScope>.Empty;
            scopeStack = scopeStack.Push(new LoggingScope<TState>(state));
            _capturedScopes.Value = scopeStack;
            return new AnonymousDisposable(() => _capturedScopes.Value = originalCapturedScopesValue!);
        }

        private sealed class LoggingScope<T> : ILoggingScope
        {
            private readonly T _value;

            public LoggingScope(T value) => _value = value;

            public IDisposable? Begin(ILogger logger) => logger?.BeginScope(_value);
        }
    }
}
