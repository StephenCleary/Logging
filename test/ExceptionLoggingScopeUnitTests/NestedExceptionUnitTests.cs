using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExceptionLoggingScopeUnitTests.Utility;
using Microsoft.Extensions.Logging;
using Nito.Logging;
using Xunit;

namespace ExceptionLoggingScopeUnitTests
{
    public class NestedExceptionUnitTests
    {
        [Fact]
        public void WrapperWithoutScopes_PropagatesInnerExceptionScopes()
        {
            var (logs, logger) = LoggingTestUtility.InitializeLogs();
            try
            {
                try
                {
                    using (logger.BeginScope("{test}", 13))
                    {
                        throw new InvalidOperationException();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException("Wrapper", ex);
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
        public void WrapperWithRethrowScope_IncludesBothScopes()
        {
            var (logs, logger) = LoggingTestUtility.InitializeLogs();
            try
            {
                try
                {
                    using (logger.BeginScope("{test}", 13))
                    {
                        throw new InvalidOperationException();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    using (logger.BeginScope("{wrapper}", 7))
                    {
                        throw new InvalidOperationException("Wrapper", ex);
                    }
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
                    Assert.Equal(7, Assert.Contains("wrapper", message.ScopeValues));
                });
        }

        [Fact]
        public void WrapperWithRethrowScope_SameKey_RethrowScopeTakesPriority()
        {
            var (logs, logger) = LoggingTestUtility.InitializeLogs();
            try
            {
                try
                {
                    using (logger.BeginScope("{test}", 13))
                    {
                        throw new InvalidOperationException();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    using (logger.BeginScope("{test}", 7))
                    {
                        throw new InvalidOperationException("Wrapper", ex);
                    }
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
        public void WrapperWithSharedScope_SameKey_ThrowScopeTakesPriority()
        {
            var (logs, logger) = LoggingTestUtility.InitializeLogs();
            try
            {
                using (logger.BeginScope("{test}", 7))
                {
                    try
                    {
                        using (logger.BeginScope("{test}", 13))
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new InvalidOperationException("Wrapper", ex);
                    }
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
        public void WrapperWithSharedScopeAndRethrowScope_SameKey_RethrowScopeTakesPriority()
        {
            var (logs, logger) = LoggingTestUtility.InitializeLogs();
            try
            {
                using (logger.BeginScope("{test}", 7))
                {
                    try
                    {
                        using (logger.BeginScope("{test}", 13))
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        using (logger.BeginScope("{test}", 5))
                        {
                            throw new InvalidOperationException("Wrapper", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (logger.BeginCapturedExceptionLoggingScopes(ex))
                    logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message => Assert.Equal(5, Assert.Contains("test", message.ScopeValues)));
        }
    }
}
