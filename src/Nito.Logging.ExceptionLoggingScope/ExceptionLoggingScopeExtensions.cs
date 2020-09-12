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

#pragma warning disable CA2000 // Dispose objects before losing scope
            var loggingScopes = new LoggingScopes();
            var subscriber = new ExceptionLoggingScopesSubscriber(loggingScopes);
            services.AddSingleton(subscriber);
            services.AddSingleton<ILoggerProvider>(new LoggingScopeTrackingLoggerProvider(loggingScopes));
#pragma warning restore CA2000 // Dispose objects before losing scope
            return services;
        }

        /// <summary>
        /// Applies any scopes captured on the specified exception.
        /// </summary>
        /// <param name="logger">The logger on which to apply the scopes. May not be <c>null</c>.</param>
        /// <param name="exception">The exception holding the captured scopes. May be <c>null</c>.</param>
        public static IDisposable? BeginCapturedExceptionLoggingScopes(this ILogger logger, Exception? exception)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            var scopes = exception?.TryGetLoggingScopes();
            if (scopes == null)
                return null;

            // TODO: don't reapply scopes that are still current.

            // TODO: if there are current scopes *past* what is captured, those should take priority.

            var disposable = new CollectionDisposable();
            foreach (var scope in scopes.Reverse())
            {
                var innerScope = scope.Begin(logger);
                if (innerScope != null)
                    disposable.Add(innerScope);
            }

            return disposable;
        }
    }
}
