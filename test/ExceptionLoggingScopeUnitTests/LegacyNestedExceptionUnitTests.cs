using System;
using ExceptionLoggingScopeUnitTests.Utility;
using Microsoft.Extensions.Logging;
using Nito.Logging;
using Xunit;

namespace ExceptionLoggingScopeUnitTests;

public class LegacyNestedExceptionUnitTests
{
    [Fact]
    public void WrapperWithoutScopes_PropagatesInnerExceptionScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
    });

    [Fact]
    public void RethrowWithoutScopes_PropagatesInnerExceptionScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
                throw;
            }
        }
        catch (Exception ex)
        {
            using (logger.BeginCapturedExceptionLoggingScopes(ex))
                logger.LogError("message");
        }

        Assert.Collection(logs.Messages,
            message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
    });

    [Fact]
    public void WrapperWithRethrowScope_IncludesBothScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
    });

    [Fact]
    public void RethrowWithRethrowScope_IncludesBothScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
                    throw;
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
    });

    [Fact]
    public void WrapperWithRethrowScope_SameKey_RethrowScopeTakesPriority() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
    });

    [Fact]
    public void RethrowWithRethrowScope_SameKey_RethrowScopeTakesPriority() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
                    throw;
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
    });

    [Fact]
    public void WrapperWithSharedScope_SameKey_ThrowScopeTakesPriority() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
    });

    [Fact]
    public void ThrowWithSharedScope_SameKey_ThrowScopeTakesPriority() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
                    throw;
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
    });

    [Fact]
    public void WrapperWithSharedScopeAndRethrowScope_SameKey_RethrowScopeTakesPriority() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
    });

    [Fact]
    public void RethrowWithSharedScopeAndRethrowScope_SameKey_RethrowScopeTakesPriority() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
                        throw;
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
    });

    [Fact]
    public void UnthrownWrapperWithoutScopes_PropagatesInnerExceptionScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
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
            using (logger.BeginCapturedExceptionLoggingScopes(new InvalidOperationException("wrapper", ex)))
                logger.LogError("message");
        }

        Assert.Collection(logs.Messages,
            message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
    });

    [Fact]
    public void UnthrownWrapperWithSharedScopes_PropagatesInnerExceptionScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
        using (logger.BeginScope("{outer}", 7))
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
                using (logger.BeginCapturedExceptionLoggingScopes(new InvalidOperationException("wrapper", ex)))
                    logger.LogError("message");
            }
        }

        Assert.Collection(logs.Messages,
            message =>
            {
                Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                Assert.Equal(7, Assert.Contains("outer", message.ScopeValues));
            });
    });

    [Fact]
    public void UnthrownWrapperWithSharedScopes_SameKey_TakesExceptionScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
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
            catch (Exception ex)
            {
                using (logger.BeginCapturedExceptionLoggingScopes(new InvalidOperationException("wrapper", ex)))
                    logger.LogError("message");
            }
        }

        Assert.Collection(logs.Messages,
            message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
    });

    [Fact]
    public void UnthrownWrapperWithLocalScope_IncludesBothScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
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
            using (logger.BeginScope("{outer}", 7))
            using (logger.BeginCapturedExceptionLoggingScopes(new InvalidOperationException("wrapper", ex)))
                logger.LogError("message");
        }

        Assert.Collection(logs.Messages,
            message =>
            {
                Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                Assert.Equal(7, Assert.Contains("outer", message.ScopeValues));
            });
    });

    [Fact]
    public void UnthrownWrapperWithLocalScope_SameKey_ExceptionScopesLast_TakesExceptionScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
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
            using (logger.BeginScope("{test}", 7))
            using (logger.BeginCapturedExceptionLoggingScopes(new InvalidOperationException("wrapper", ex)))
                logger.LogError("message");
        }

        Assert.Collection(logs.Messages,
            message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
    });

    [Fact]
    public void UnthrownWrapperWithLocalScope_SameKey_LocalScopesLast_TakesLocalScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
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
            using (logger.BeginCapturedExceptionLoggingScopes(new InvalidOperationException("wrapper", ex)))
            using (logger.BeginScope("{test}", 7))
                logger.LogError("message");
        }

        Assert.Collection(logs.Messages,
            message => Assert.Equal(7, Assert.Contains("test", message.ScopeValues)));
    });
}
