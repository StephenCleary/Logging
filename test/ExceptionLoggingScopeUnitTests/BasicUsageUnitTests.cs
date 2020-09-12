using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using ExceptionLoggingScopeUnitTests.Utility;
using Microsoft.Extensions.Logging;
using Nito.Logging;
using Xunit;

namespace ExceptionLoggingScopeUnitTests
{
    public class BasicUsageUnitTests
    {
        [Fact]
        public void ExceptionScope_IsCaptured()
        {
            var (logs, logger) = InitializeLogs();
            try
            {
                using (logger.BeginScope("{test}", 13))
                {
                    throw new InvalidOperationException();
                }
            }
            catch (Exception ex)
            {
                using (logger.BeginCapturedExceptionLoggingScopes(ex))
                    logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message => Assert.Collection(message.ScopeValues, kvp =>
                {
                    Assert.Equal("test", kvp.Key);
                    Assert.Equal(13, kvp.Value);
                }));
        }

        private static (InMemoryLoggerProvider Logs, ILogger Logger) InitializeLogs()
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
