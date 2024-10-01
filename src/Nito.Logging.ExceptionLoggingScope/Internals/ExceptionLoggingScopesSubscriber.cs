using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Nito.Logging.Internals;

/// <summary>
/// A type that monitors the current <see cref="AppDomain"/> for exceptions and attaches logging scopes to them when thrown.
/// </summary>
public sealed class ExceptionLoggingScopesSubscriber : IDisposable
{
    private readonly EventHandler<FirstChanceExceptionEventArgs> _subscription;

/// <summary>
/// Constructor.
/// </summary>
public ExceptionLoggingScopesSubscriber(CaptureLoggingScopesLoggerProvider loggerProvider)
    {
        _subscription = (_, args) =>
        {
            var exceptionScopes = args.Exception.TryFindLoggingScopes() ?? [];
            var currentScopes = loggerProvider.TryGetCurrentScopes() ?? [];

            IReadOnlyList<object> combinedScopes = [..exceptionScopes, ..currentScopes.Where(x => !exceptionScopes.Contains(x))];

            if (combinedScopes.Count != 0)
                args.Exception.SetLoggingScopes(combinedScopes);
        };
        AppDomain.CurrentDomain.FirstChanceException += _subscription;
}

    /// <summary>
    /// No longer attaches scopes to exceptions.
    /// </summary>
    public void Dispose() => AppDomain.CurrentDomain.FirstChanceException -= _subscription;
}
