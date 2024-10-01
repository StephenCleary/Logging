using System;
using System.Collections.Generic;
using Nito.ConnectedProperties;

namespace Nito.Logging.Internals;

/// <summary>
/// Extension types for scopes on exceptions.
/// </summary>
public static class ExceptionExtensions
{
    private static readonly string _loggingScopesName = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Sets the logging scopes for an exception.
    /// </summary>
    public static void SetLoggingScopes(this Exception exception, IReadOnlyList<object> loggingScopes)
    {
        ConnectedProperty.GetConnectedProperty(exception, _loggingScopesName, bypassValidation: true).Set(loggingScopes);
    }

    /// <summary>
    /// Attempts to get the logging scopes for an exception. Returns <c>null</c> if there are no logging scopes for this exception.
    /// </summary>
    public static IReadOnlyList<object>? TryGetLoggingScopes(this Exception exception)
    {
        var prop = ConnectedProperty.GetConnectedProperty(exception, _loggingScopesName, bypassValidation: true);
        if (prop == null)
            return null;
        if (!prop.TryGet(out var value))
            return null;
        return value as IReadOnlyList<object>;
    }

    /// <summary>
    /// Attempts to find the logging scopes on an exception or its chain of single inner exceptions. Return <c>null</c> if no logging scopes were found.
    /// </summary>
    public static IReadOnlyList<object>? TryFindLoggingScopes(this Exception? exception)
    {
        while (exception != null)
        {
            var scopes = exception.TryGetLoggingScopes();
            if (scopes != null)
                return scopes;
            exception = exception.TryGetSingleInnerException();
        }

        return null;
    }

    /// <summary>
    /// Gets the inner exception, unless there are multiple inner exceptions.
    /// Returns <c>null</c> if there are zero or multiple inner exceptions.
    /// </summary>
    public static Exception? TryGetSingleInnerException(this Exception exception)
    {
        _ = exception ?? throw new ArgumentNullException(nameof(exception));
        if (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count != 1)
            return null;
        return exception.InnerException;
    }
}
