using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Nito.ConnectedProperties;

namespace Nito.Logging.Internals
{
    /// <summary>
    /// Extension types for scopes on exceptions.
    /// </summary>
    public static class ExceptionExtensions
    {
        private static readonly string ScopesName = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Sets the scopes for an exception.
        /// </summary>
        public static void SetScopes(this Exception exception, ImmutableStack<ILoggingScope> scopes)
        {
            ConnectedProperty.GetConnectedProperty(exception, ScopesName, bypassValidation: true).Set(scopes);
        }

        /// <summary>
        /// Attempts to get the scopes for an exception. Returns <c>null</c> if there are no scopes for this exception.
        /// </summary>
        public static ImmutableStack<ILoggingScope>? TryGetScopes(this Exception exception)
        {
            var prop = ConnectedProperty.GetConnectedProperty(exception, ScopesName, bypassValidation: true);
            if (prop == null)
                return null;
            if (!prop.TryGet(out var value))
                return null;
            return value as ImmutableStack<ILoggingScope>;
        }
    }
}
