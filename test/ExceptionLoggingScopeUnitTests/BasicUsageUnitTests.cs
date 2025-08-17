using System;
using ExceptionLoggingScopeUnitTests.Utility;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ExceptionLoggingScopeUnitTests;

public class BasicUsageUnitTests
{
    [Fact]
    public void ThrowScope_IsCaptured() => LoggingTestUtility.InitializeLogs((logs, logger) =>
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
            logger.LogError(ex, "message");
        }

        Assert.Collection(logs.Messages,
            message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
    });

    [Fact]
    public void ThrowScope_WhenNested_CapturesBoth() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
            logger.LogError(ex, "message");
        }

        Assert.Collection(logs.Messages,
            message =>
            {
                Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                Assert.Equal(7, Assert.Contains("test2", message.ScopeValues));
            });
    });

    [Fact]
    public void ThrowScope_WhenNestedWithSameKey_InnerOverridesOuter() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
            logger.LogError(ex, "message");
        }

        Assert.Collection(logs.Messages,
            message => Assert.Equal(7, Assert.Contains("test", message.ScopeValues)));
    });

    [Fact]
    public void ThrowScope_WithSharedScope_CapturesBothScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
                logger.LogError(ex, "message");
            }
        }

        Assert.Collection(logs.Messages,
            message =>
            {
                Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                Assert.Equal(11, Assert.Contains("shared", message.ScopeValues));
            });
    });

    [Fact]
    public void ThrowScope_WhenSharedScopeHasSameKey_ThrowScopeOverridesSharedScope() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
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
                logger.LogError(ex, "message");
            }
        }

        Assert.Collection(logs.Messages,
            message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
    });

    [Fact]
    public void ThrowScope_WithLogScope_AppliesBothScopes() => LoggingTestUtility.InitializeLogs((logs, logger) =>
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
            using (logger.BeginScope("{test2}", 7))
                logger.LogError(ex, "message");
        }

        Assert.Collection(logs.Messages,
            message =>
            {
                Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                Assert.Equal(7, Assert.Contains("test2", message.ScopeValues));
            });
    });

    [Fact]
    public void ThrowScope_WhenLogScopeHasSameKeyAndIsFirst_ThrowScopeOverridesLogScope() => LoggingTestUtility.InitializeLogs((logs, logger) =>
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
                logger.LogError(ex, "message");
        }

        Assert.Collection(logs.Messages,
            message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
    });
}
