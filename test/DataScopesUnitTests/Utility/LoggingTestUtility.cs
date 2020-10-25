using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestUtilities;

namespace DataScopesUnitTests.Utility
{
    public class LoggingTestUtility
    {
        public static void InitializeLogs(Action<InMemoryLoggerProvider, ILogger> action)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var logs = new InMemoryLoggerProvider();
            services.AddSingleton<ILoggerProvider>(logs);
            using var provider = services.BuildServiceProvider();

            var logger = provider.GetRequiredService<ILogger<BasicUsageUnitTests>>();
            action(logs, logger);
        }
    }
}