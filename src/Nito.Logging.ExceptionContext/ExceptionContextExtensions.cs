using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.Logging.ExceptionContext.Internals;

namespace Nito.Logging.ExceptionContext
{
    /// <summary>
    /// Provides extension methods for Nito.Logging.ExceptionContext.
    /// </summary>
    public static class ExceptionContextExtensions
    {
        /// <summary>
        /// Captures logging scopes when exceptions are thrown, and applies those scopes when exceptions are logged.
        /// </summary>
        public static IHostBuilder CaptureLoggingContextForExceptions(this IHostBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<ILoggerProvider, ScopeTrackingLoggerProvider>();
            });
        }
    }
}
