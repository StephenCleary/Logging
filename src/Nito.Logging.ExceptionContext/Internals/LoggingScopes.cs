using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Nito.Disposables;

namespace Nito.Logging.Internals
{
    /// <summary>
    /// A stack of logging scopes.
    /// </summary>
    public sealed class LoggingScopes
    {
        private readonly AsyncLocal<ImmutableStack<IScope>> _capturedScopes = new AsyncLocal<ImmutableStack<IScope>>();

        /// <summary>
        /// Gets the current stack of scopes.
        /// </summary>
        public ImmutableStack<IScope> CurrentScopes => _capturedScopes.Value ?? ImmutableStack<IScope>.Empty;

        /// <summary>
        /// Pushes a logging scope onto the stack. Returns a disposable that pops this logging scope when disposed.
        /// </summary>
        public IDisposable PushLoggingScope<TState>(TState state)
        {
            var originalCapturedScopesValue = _capturedScopes.Value;
            var scopeStack = originalCapturedScopesValue ?? ImmutableStack<IScope>.Empty;
            scopeStack = scopeStack.Push(new Scope<TState>(state));
            _capturedScopes.Value = scopeStack;
            return new AnonymousDisposable(() => _capturedScopes.Value = originalCapturedScopesValue!);
        }
    }
}
