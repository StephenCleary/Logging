2.0.0

- [Major] Dropped support for .NET Framework 4.6.1. .NET Framework 4.6.2 is still supported.
- [Major] Simplified usage of exception logging scopes.
- [Feature] Upgraded dependencies.
- [Feature] Prepared code for trimming/single-file/AOT support (`net8.0` only). Trimming/single-file/AOT is not fully supported in this release.

1.0.1

- [Fix] Workaround for Application Insights. Every other logging system in the world uses `IEnumerable<KeyValuePair<string, object>>`, but Application Insights only takes `IReadOnlyList<KeyValuePair<string, object>>`. So be it, Jedi.

1.0.0

- Initial release.