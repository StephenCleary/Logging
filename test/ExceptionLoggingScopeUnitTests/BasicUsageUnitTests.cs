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
        public void ThrowScope_IsCaptured()
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
                message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
        }

        [Fact]
        public void ThrowScope_WhenNested_CapturesBoth()
        {
            var (logs, logger) = InitializeLogs();
            try
            {
                using (logger.BeginScope("{test}", 13))
                using (logger.BeginScope("{test2}", 7))
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
                message =>
                {
                    Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                    Assert.Equal(7, Assert.Contains("test2", message.ScopeValues));
                });
        }

        [Fact]
        public void ThrowScope_WhenNestedWithSameKey_InnerOverridesOuter()
        {
            var (logs, logger) = InitializeLogs();
            try
            {
                using (logger.BeginScope("{test}", 13))
                using (logger.BeginScope("{test}", 7))
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
                message => Assert.Equal(7, Assert.Contains("test", message.ScopeValues)));
        }

        [Fact]
        public void ThrowScope_WithSharedScope_CapturesBothScopes()
        {
            var (logs, logger) = InitializeLogs();
            using (logger.BeginScope("{shared}", 11))
            {
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
            }

            Assert.Collection(logs.Messages,
                message =>
                {
                    Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                    Assert.Equal(11, Assert.Contains("shared", message.ScopeValues));
                });
        }

        [Fact]
        public void ThrowScope_WhenSharedScopeHasSameKey_ThrowScopeOverridesSharedScope()
        {
            var (logs, logger) = InitializeLogs();
            using (logger.BeginScope("{test}", 11))
            {
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
            }

            Assert.Collection(logs.Messages,
                message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
        }

        [Fact]
        public void ThrowScope_WithLogScope_AppliesBothScopes()
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
                using (logger.BeginScope("{test2}", 7))
                using (logger.BeginCapturedExceptionLoggingScopes(ex))
                    logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message =>
                {
                    Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                    Assert.Equal(7, Assert.Contains("test2", message.ScopeValues));
                });
        }

        [Fact]
        public void ThrowScope_WhenLogScopeHasSameKeyAndIsFirst_ThrowScopeOverridesLogScope()
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
                using (logger.BeginScope("{test}", 7))
                using (logger.BeginCapturedExceptionLoggingScopes(ex))
                    logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
        }

        [Fact]
        public void ThrowScope_WhenLogScopeHasSameKeyAndIsLast_LogScopeOverridesThrowScope()
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
                using (logger.BeginScope("{test}", 7))
                    logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message => Assert.Equal(7, Assert.Contains("test", message.ScopeValues)));
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
