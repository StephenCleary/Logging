![Logo](src/icon.png)

# Nito.Logging [![Build status](https://github.com/StephenCleary/Logging/workflows/Build/badge.svg)](https://github.com/StephenCleary/Logging/actions?query=workflow%3ABuild) [![codecov](https://codecov.io/gh/StephenCleary/Logging/branch/master/graph/badge.svg)](https://codecov.io/gh/StephenCleary/Logging) [![NuGet version](https://badge.fury.io/nu/Nito.Logging.svg)](https://www.nuget.org/packages/Nito.Logging) [![API docs](https://img.shields.io/badge/API-dotnetapis-blue.svg)](http://dotnetapis.com/pkg/Nito.Logging)

A library for using scopes with Microsoft.Extensions.Logging.

# What It Does

This library:
1. Captures logging scopes when an exception is thrown.
2. Applies those logging scopes when that exception is logged.

In other words, if you find your exception logs are missing useful information from your logging contexts, install this library and enjoy rich logs again!

# Getting Started

Three simple steps:
1. Install [the `Nito.Logging` package](https://www.nuget.org/packages/Nito.Logging).
1. Add a call to `AddExceptionLoggingScopes()` in your service registration.
1. Wrap all exception logging calls in `ILogger.BeginCapturedExceptionLoggingScopes(Exception)`.
1. Your exception logs now have logging contexts. Treat yourself to ice cream!

# Alternatives

- [Throw context enricher for Serilog](https://github.com/Tolyandre/serilog-throw-context-enricher) - similar approach, for Serilog only.
