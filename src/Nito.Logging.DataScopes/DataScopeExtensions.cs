using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nito.Logging
{
    /// <summary>
    /// Provides extension methods for defining data scopes.
    /// </summary>
    public static class DataScopeExtensions
    {
        /// <summary>
        /// Adds a collection of key/value data pairs to the current logging scope.
        /// </summary>
        public static IDisposable BeginDataScope(this ILogger logger, IEnumerable<KeyValuePair<string, object?>> data)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
            return logger.BeginScope(new LogData(data));
        }

        /// <summary>
        /// Adds a collection of key/value data pairs to the current logging scope.
        /// </summary>
        public static IDisposable BeginDataScope(this ILogger logger, IEnumerable<(string Key, object? Value)> data) =>
            logger.BeginDataScope(data.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)));

        /// <summary>
        /// Adds any number of key/value pairs to the current logging scope.
        /// </summary>
        public static IDisposable BeginDataScope(this ILogger logger, params (string Key, object? Value)[] data) =>
            logger.BeginDataScope(data.AsEnumerable());

        /// <summary>
        /// Adds a single key/value pair to the current logging scope.
        /// </summary>
        public static IDisposable BeginDataScope(this ILogger logger, (string Key, object? Value) data) =>
            logger.BeginDataScope(new[] {data});

        /// <summary>
        /// Adds the properties of an object to the current logging scope.
        /// </summary>
        public static IDisposable BeginDataScope(this ILogger logger, object data)
        {
            _ = data ?? throw new ArgumentNullException(nameof(data));
            // TODO: ensure dictionaries don't end up here.
            return logger.BeginDataScope(AnonymousObjectData(data));
        }

        private static IEnumerable<KeyValuePair<string, object?>> AnonymousObjectData(object data)
        {
            var properties = data.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.GetIndexParameters().Length == 0 && prop.GetMethod != null);

            foreach (var prop in properties)
                yield return new KeyValuePair<string, object?>(prop.Name, prop.GetValue(data));
        }

        private sealed class LogData : IEnumerable<KeyValuePair<string, object?>>
        {
            private readonly IEnumerable<KeyValuePair<string, object?>> _data;

            public LogData(IEnumerable<KeyValuePair<string, object?>> data) => _data = data;

            public override string ToString() => string.Join(", ", _data.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() => _data.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
        }
    }
}
