using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.Disposables;
using Nito.Logging.Internals;

namespace Nito.Logging;

/// <summary>
/// Provides extension methods for Nito.Logging.ExceptionLoggingScope.
/// </summary>
public static class ExceptionLoggingScopeExtensions
{
    /// <summary>
    /// Adds the types necessary to capture logging scopes when exceptions are thrown.
    /// Use <see cref="BeginCapturedExceptionLoggingScopes"/> to retrieve the logging scopes from an exception and apply them while logging.
    /// </summary>
    public static IServiceCollection AddExceptionLoggingScopes(this IServiceCollection services)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));

        services.AddSingleton<CaptureLoggingScopesLoggerProvider>();
        services.AddSingleton<ExceptionLoggingScopesSubscriber>();
        services.AddLogging(x => x
            .AddFilter<CaptureLoggingScopesLoggerProvider>(null, LogLevel.None)
            .AddFilter<ExceptionLoggingScopesLoggerProvider>(null, LogLevel.Trace)
            .AddFilter<ExceptionLoggingScopesCustomLoggerProvider>(null, LogLevel.Trace));
        services.Decorate<ILoggerProvider>((innerLoggerProvider, serviceProvider) =>
        {
            _ = serviceProvider.GetRequiredService<ExceptionLoggingScopesSubscriber>();
            return innerLoggerProvider is ISupportExternalScope ?
                    new ExceptionLoggingScopesLoggerProvider(innerLoggerProvider, serviceProvider.GetRequiredService<IOptionsMonitor<LoggerFilterOptions>>()) :
                    new ExceptionLoggingScopesCustomLoggerProvider(innerLoggerProvider, serviceProvider.GetRequiredService<IOptionsMonitor<LoggerFilterOptions>>());
        });
        services.AddSingleton<ILoggerProvider>(provider => provider.GetRequiredService<CaptureLoggingScopesLoggerProvider>());
        return services;
    }

    /// <summary>
    /// Applies any scopes captured on the specified exception or its chain of single inner exceptions.
    /// </summary>
    /// <param name="logger">The logger on which to apply the scopes. May not be <c>null</c>.</param>
    /// <param name="exception">The exception holding the captured scopes. May be <c>null</c>.</param>
    public static IDisposable? BeginCapturedExceptionLoggingScopes(this ILogger logger, Exception? exception)
    {
#if NET462 || NETSTANDARD2_0
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));
#else
        ArgumentNullException.ThrowIfNull(logger);
#endif

        var scopes = exception?.TryFindLoggingScopes();
        if (scopes == null)
            return null;

        var disposable = new CollectionDisposable();
        foreach (var scope in scopes)
        {
            var innerScope = logger.BeginScope(scope!);
            if (innerScope != null)
                disposable.Add(innerScope);
        }

        return disposable;
    }
}
