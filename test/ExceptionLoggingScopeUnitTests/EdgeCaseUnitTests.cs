using System;
using ExceptionLoggingScopeUnitTests.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.Logging;
using TestUtilities;
using Xunit;

namespace ExceptionLoggingScopeUnitTests;

public class EdgeCaseUnitTests
{
    [Fact]
    public void BeginCapturedExceptionLoggingScopes_WithoutLogger_Throws() => LoggingTestUtility.InitializeLogs((logs, logger) =>
    {
        Assert.ThrowsAny<ArgumentException>(() =>
        {
            try
            {
                throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                ILogger myLogger = null;
                using var disposable = myLogger.BeginCapturedExceptionLoggingScopes(ex);
            }
        });
    });

    [Fact]
    public void BeginCapturedExceptionLoggingScopes_WithoutInitialization_ReturnsNull()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var logs = new InMemoryLoggerProvider();
        services.AddSingleton<ILoggerProvider>(logs);
        using var provider = services.BuildServiceProvider();

        var logger = provider.GetRequiredService<ILogger<EdgeCaseUnitTests>>();

        try
        {
            throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            using var disposable = logger.BeginCapturedExceptionLoggingScopes(ex);
            Assert.Null(disposable);
        }
    }
}
