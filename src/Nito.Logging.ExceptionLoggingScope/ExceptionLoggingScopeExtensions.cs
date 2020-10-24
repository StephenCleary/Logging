using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.Disposables;
using Nito.Logging.Internals;

namespace Nito.Logging
{
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

            var loggingScopes = new LoggingScopes();
            services.AddSingleton(_ => new ExceptionLoggingScopesSubscriber(loggingScopes));
            services.AddSingleton<ILoggerProvider>(provider =>
            {
                _ = provider.GetService<ExceptionLoggingScopesSubscriber>();
                return new LoggingScopeTrackingLoggerProvider(loggingScopes);
            });
            return services;
        }

        /// <summary>
        /// Applies any scopes captured on the specified exception or its chain of single inner exceptions.
        /// </summary>
        /// <param name="logger">The logger on which to apply the scopes. May not be <c>null</c>.</param>
        /// <param name="exception">The exception holding the captured scopes. May be <c>null</c>.</param>
        public static IDisposable? BeginCapturedExceptionLoggingScopes(this ILogger logger, Exception? exception)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            var scopes = exception?.TryFindLoggingScopes()?.Reverse();
            if (scopes == null)
                return null;

            var disposable = new CollectionDisposable();
            foreach (var scope in scopes)
            {
                var innerScope = scope.Begin(logger);
                if (innerScope != null)
                    disposable.Add(innerScope);
            }

            return disposable;
        }
    }
}
