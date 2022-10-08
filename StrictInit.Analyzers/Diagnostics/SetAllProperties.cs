using Microsoft.CodeAnalysis;

namespace StrictInit.Analyzers.Diagnostics;

internal static class SetAllProperties
{
    public const string Id = "SI001";

    private static readonly string Title = "Set all properties";
    private static readonly string MessageFormat = "Public property {0} not set";
    private static readonly string Description = "Consider setting other public properties in object initializer.";
    private const string Category = "Usage";

    public static readonly DiagnosticDescriptor Rule = new(Id,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);
}