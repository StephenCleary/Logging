1.0.1

- Workaround for Application Insights. Every other logging system in the world uses `IEnumerable<KeyValuePair<string, object>>`, but Application Insights only takes `IReadOnlyList<KeyValuePair<string, object>>`. So be it, Jedi.

1.0.0

- Initial release.