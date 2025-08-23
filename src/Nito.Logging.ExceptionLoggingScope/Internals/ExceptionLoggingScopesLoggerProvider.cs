using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Nito.Logging.Internals;

/// <summary>
/// A logging provider that applies captured exception logging scopes when logging exceptions to an inner logging provider.
/// </summary>
public sealed class ExceptionLoggingScopesLoggerProvider(
    ILoggerProvider innerLoggerProvider,
    IOptionsMonitor<LoggerFilterOptions> optionsMonitor)
    : WrappingLoggerProviderBase(innerLoggerProvider, optionsMonitor), ISupportExternalScope
{
    void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider) =>
        ((ISupportExternalScope)InnerLoggerProvider).SetScopeProvider(scopeProvider);

    /// <inheritdoc/>
    protected override LoggerBase CreateLogger(string categoryName, ILogger innerLogger)
        => new Logger(this, categoryName, innerLogger);

    private sealed class Logger(WrappingLoggerProviderBase provider, string categoryName, ILogger innerLogger)
        : LoggerBase(provider, categoryName, innerLogger)
    {
        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            using var scope = InnerLogger.BeginCapturedExceptionLoggingScopes(exception);
            base.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
