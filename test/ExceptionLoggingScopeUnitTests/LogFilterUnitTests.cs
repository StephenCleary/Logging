using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.Logging;
using TestUtilities;
using Xunit;

namespace ExceptionLoggingScopeUnitTests;

public class LogFilterUnitTests
{
    [Fact]
    public void CustomTraceLogLevel_IsHonored()
    {
        // https://github.com/StephenCleary/Logging/issues/1

        var services = new ServiceCollection();
        services.AddLogging(c => c.AddFilter<InMemoryLoggerProvider>(null, LogLevel.Trace));
        var logs = new InMemoryLoggerProvider();
        services.AddSingleton<ILoggerProvider>(logs);
        services.AddExceptionLoggingScopes();
        using var provider = services.BuildServiceProvider();

        var logger = provider.GetRequiredService<ILogger<LogFilterUnitTests>>();

        logger.LogTrace("Test");

        Assert.Single(logs.Messages, log => log.Message == "Test");
    }

    [Fact]
    public void CustomWarningLogLevel_IsHonored()
    {
        var services = new ServiceCollection();
        services.AddLogging(c => c.AddFilter<InMemoryLoggerProvider>(null, LogLevel.Warning));
        var logs = new InMemoryLoggerProvider();
        services.AddSingleton<ILoggerProvider>(logs);
        services.AddExceptionLoggingScopes();
        using var provider = services.BuildServiceProvider();

        var logger = provider.GetRequiredService<ILogger<LogFilterUnitTests>>();

        logger.LogInformation("Should not appear");
        logger.LogWarning("Test");

        var observed = Assert.Single(logs.Messages);
        Assert.Equal("Test", observed.Message);
    }

    [Fact]
    public void CustomCategory_IsHonored()
    {
        var services = new ServiceCollection();
        services.AddLogging(c => c
            .AddFilter<InMemoryLoggerProvider>("cat1", LogLevel.Warning)
            .AddFilter<InMemoryLoggerProvider>("cat2", LogLevel.Information));
        var logs = new InMemoryLoggerProvider();
        services.AddSingleton<ILoggerProvider>(logs);
        services.AddExceptionLoggingScopes();
        using var provider = services.BuildServiceProvider();

        var logger1 = provider.GetRequiredService<ILoggerFactory>().CreateLogger("cat1");
        var logger2 = provider.GetRequiredService<ILoggerFactory>().CreateLogger("cat2");

        logger1.LogInformation("Should not appear");
        logger2.LogInformation("Test");

        var observed = Assert.Single(logs.Messages);
        Assert.Equal("Test", observed.Message);
    }

    [Fact]
    public void CustomFilterDelegate_IsHonored()
    {
        var services = new ServiceCollection();
        var invoked = false;
        services.AddLogging(c => c.AddFilter<InMemoryLoggerProvider>((_, _) =>
        {
            invoked = true;
            return false;
        }));
        var logs = new InMemoryLoggerProvider();
        services.AddSingleton<ILoggerProvider>(logs);
        services.AddExceptionLoggingScopes();
        using var provider = services.BuildServiceProvider();

        var logger = provider.GetRequiredService<ILogger<LogFilterUnitTests>>();

        logger.LogInformation("Should not appear");

        Assert.Empty(logs.Messages);
        Assert.True(invoked);
    }
}
