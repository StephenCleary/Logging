using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Nito.Logging.Internals;

/// <summary>
/// A logging provider that just tracks logging scopes.
/// </summary>
public sealed class CaptureLoggingScopesLoggerProvider : ILoggerProvider, ISupportExternalScope
{
	private readonly object _mutex = new();
	private IExternalScopeProvider? _externalScopeProvider;

	/// <summary>
	/// Retrieves the logging scopes that are currently active.
	/// </summary>
	public IReadOnlyList<object>? TryGetCurrentScopes()
	{
		var externalScopeProvider = TryGetExternalScopeProvider();
		if (externalScopeProvider == null)
			return null;

		List<object> scopeValues = [];
		externalScopeProvider.ForEachScope(AddScope, scopeValues);
		return scopeValues;

		static void AddScope(object? scope, List<object> list) => list.Add(scope!);
	}

	/// <inheritdoc/>
	public void Dispose() { }

	void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
	{
		lock (_mutex)
			_externalScopeProvider = scopeProvider;
	}

	ILogger ILoggerProvider.CreateLogger(string categoryName) => new Logger(this);

	private IExternalScopeProvider? TryGetExternalScopeProvider()
	{
		lock (_mutex)
			return _externalScopeProvider;
	}

	private sealed class Logger(CaptureLoggingScopesLoggerProvider provider) : ILogger
    {
        private readonly CaptureLoggingScopesLoggerProvider _provider = provider;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

        public bool IsEnabled(LogLevel logLevel) => false;

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
			_provider.TryGetExternalScopeProvider()?.Push(state)!;
	}
}
