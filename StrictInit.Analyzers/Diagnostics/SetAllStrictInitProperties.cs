using Microsoft.CodeAnalysis;

namespace StrictInit.Analyzers.Diagnostics;

internal static class SetAllStrictInitProperties
{
    public const string Id = "SI002";

    private static readonly string Title = "Set all StrictInit properties";
    private static readonly string MessageFormat = "Public property {0} not set";
    private static readonly string Description = "All properties marked with StrictInit attribute should be set";
    private const string Category = "Usage";

    public static readonly DiagnosticDescriptor Rule = new(Id,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);
}