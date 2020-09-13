using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.Logging;

namespace ExceptionLoggingScopeUnitTests.Utility
{
    public class LoggingTestUtility
    {
        public static (InMemoryLoggerProvider Logs, ILogger Logger) InitializeLogs()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var logs = new InMemoryLoggerProvider();
            services.AddSingleton<ILoggerProvider>(logs);
            services.AddExceptionLoggingScopes();
            var provider = services.BuildServiceProvider();

            var logger = provider.GetRequiredService<ILogger<BasicUsageUnitTests>>();
            return (logs, logger);
        }
    }
}