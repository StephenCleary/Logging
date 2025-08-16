![Logo](src/icon.png)

# Nito.Logging [![Build status](https://github.com/StephenCleary/Logging/workflows/Build/badge.svg)](https://github.com/StephenCleary/Logging/actions?query=workflow%3ABuild) [![codecov](https://codecov.io/gh/StephenCleary/Logging/branch/main/graph/badge.svg)](https://codecov.io/gh/StephenCleary/Logging) [![NuGet version](https://badge.fury.io/nu/Nito.Logging.svg)](https://www.nuget.org/packages/Nito.Logging) [![API docs](https://img.shields.io/badge/API-FuGet-blue.svg)](https://fuget.org/packages/Nito.Logging)

A library for using scopes with Microsoft.Extensions.Logging.

# What It Does

This library has two parts:
1. `DataScopes` provides the `BeginDataScope` extension methods for `ILogger`, which allow you to attach name/value pairs of metadata onto your log messages.
1. `ExceptionLoggingScope` captures logging scopes when an exception is thrown, and applies those logging scopes when the exception is logged. In other words, if you find your exception logs are missing useful information from your logging contexts, install this library and enjoy rich logs again!

# Getting Started

First, install [the `Nito.Logging` package](https://www.nuget.org/packages/Nito.Logging).

To attach data scopes to your logs, call `BeginDataScope` on any `ILogger` or `ILogger<T>`. You can pass an anonymous object, any number of `(string, object)` tuples, or a collection of `KeyValuePair<string, object>` (such as a `Dictionary<string, object>`). See [the unit tests](https://github.com/StephenCleary/Logging/blob/main/test/DataScopesUnitTests/BasicUsageUnitTests.cs) for examples.

To preserve logging scopes for exceptions, add a call to `AddExceptionLoggingScopes()` in your service registration _after_ all loggers have been configured.

Your service registration will look something like this:

```C#
public void ConfigureServices(IServiceCollection services)
{
    /* Configure all loggers here */
    ...
    services.AddExceptionLoggingScopes();
    ...
}
```

Now all logs of exceptions will include the logging scopes at the point they were thrown!

# Alternatives

- [Throw context enricher for Serilog](https://github.com/Tolyandre/serilog-throw-context-enricher) - essentially the same as `ExceptionLoggingScope`, but for Serilog only.
