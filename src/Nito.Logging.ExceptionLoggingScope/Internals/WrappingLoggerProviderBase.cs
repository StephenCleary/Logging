using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Nito.Logging.Internals;

/// <summary>
/// A logging provider that wraps a given logging provider, maintaining proper filter behavior.
/// </summary>
public abstract class WrappingLoggerProviderBase: ILoggerProvider
{
    /// <summary>
    /// Creates a wrapping logger provider.
    /// </summary>
    protected WrappingLoggerProviderBase(ILoggerProvider innerLoggerProvider,
        IOptionsMonitor<LoggerFilterOptions> optionsMonitor)
    {
        _ = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));

        InnerLoggerProvider = innerLoggerProvider;
        _filterOptions = optionsMonitor.CurrentValue;
        _changeToken = optionsMonitor.OnChange(OnChange);
        OnChange(optionsMonitor.CurrentValue);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _changeToken?.Dispose();
        InnerLoggerProvider.Dispose();
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, category =>
        {
            var result = CreateLogger(categoryName, InnerLoggerProvider.CreateLogger(categoryName));

            lock (_mutex)
            {
                UpdateLoggerFilter_NoLock(result, category);
            }

            return result;
        });

    /// <summary>
    /// Returns the wrapped logger provider.
    /// </summary>
    protected ILoggerProvider InnerLoggerProvider { get; }

    /// <summary>
    /// Creates a logger that wraps the specified inner logger.
    /// </summary>
    protected abstract LoggerBase CreateLogger(string categoryName, ILogger innerLogger);

    private void OnChange(LoggerFilterOptions options)
    {
        lock (_mutex)
        {
            _filterOptions = options;
            foreach (var kvp in _loggers)
                UpdateLoggerFilter_NoLock(kvp.Value, kvp.Key);
        }
    }

    private void UpdateLoggerFilter_NoLock(LoggerBase logger, string category)
    {
        LoggerRuleSelector.Select(_filterOptions,
            InnerLoggerProvider.GetType(),
            category,
            out var minLevel,
            out var filter);
        logger.UpdateFilter(minLevel, filter);
    }

    private readonly IDisposable? _changeToken;
    private readonly object _mutex = new();
    private readonly ConcurrentDictionary<string, LoggerBase> _loggers = [];
    private LoggerFilterOptions _filterOptions;

    /// <summary>
    /// A logger that wraps an inner logger.
    /// </summary>
    protected abstract class LoggerBase(WrappingLoggerProviderBase provider, string category, ILogger innerLogger) : ILogger
    {
        /// <inheritdoc/>
        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsFilterEnabled(logLevel))
                return;
            InnerLogger.Log(logLevel, eventId, state, exception, formatter);
        }

        /// <inheritdoc/>
        public virtual bool IsEnabled(LogLevel logLevel) => IsFilterEnabled(logLevel) && InnerLogger.IsEnabled(logLevel);

        /// <inheritdoc/>
        public virtual IDisposable? BeginScope<TState>(TState state) where TState : notnull => InnerLogger.BeginScope(state);

        /// <summary>
        /// Returns whether logging is enabled for the specified level, based on the current filter settings.
        /// </summary>
        public bool IsFilterEnabled(LogLevel level)
        {
            LogLevel? minLevel;
            Func<string?, string?, LogLevel, bool>? filter;
            lock (_mutex)
            {
                minLevel = _minLevel;
                filter = _filter;
            }

            // See https://github.com/dotnet/runtime/blob/664eda9afd07db4a1998bb333512ba9def3ec18f/src/libraries/Microsoft.Extensions.Logging/src/LoggerInformation.cs#L30
            if (minLevel != null && level < minLevel)
                return false;
            if (filter != null)
                return filter(WrappedProviderTypeFullName, Category, level);
            return true;
        }

        /// <summary>
        /// Updates the filter settings for this logger.
        /// </summary>
        public void UpdateFilter(LogLevel? minLevel, Func<string?, string?, LogLevel, bool>? filter)
        {
            lock (_mutex)
            {
                _minLevel = minLevel;
                _filter = filter;
            }
        }

        /// <summary>
        /// The category name for this logger.
        /// </summary>
        public string Category { get; } = category;

        /// <summary>
        /// Returns the wrapped logger.
        /// </summary>
        protected ILogger InnerLogger { get; } = innerLogger;

        /// <summary>
        /// The full name of the wrapped logging provider.
        /// </summary>
        protected string? WrappedProviderTypeFullName { get; } = provider.InnerLoggerProvider.GetType().FullName;

        private readonly object _mutex = new();
        private LogLevel? _minLevel;
        private Func<string?, string?, LogLevel, bool>? _filter;
    }
}
