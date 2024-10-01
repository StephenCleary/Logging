using System;
using Microsoft.Extensions.Logging;

namespace Nito.Logging.Internals;

/// <summary>
/// A logging provider that applies captured exception logging scopes when logging exceptions to an inner logging provider.
/// </summary>
public sealed class ExceptionLoggingScopesCustomLoggerProvider(ILoggerProvider innerLoggerProvider) : ILoggerProvider
{
	private readonly ILoggerProvider _innerLoggerProvider = innerLoggerProvider;

	/// <inheritdoc/>
	public void Dispose() => _innerLoggerProvider.Dispose();

	ILogger ILoggerProvider.CreateLogger(string categoryName) => new Logger(_innerLoggerProvider.CreateLogger(categoryName));

	private sealed class Logger(ILogger innerLogger) : ILogger
    {
		private readonly ILogger _innerLogger = innerLogger;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
			using var scope = _innerLogger.BeginCapturedExceptionLoggingScopes(exception);
			_innerLogger.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _innerLogger.BeginScope(state);
	}
}
