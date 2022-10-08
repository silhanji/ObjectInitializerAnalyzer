using System.Linq;
using Microsoft.CodeAnalysis;

namespace StrictInit.Analyzers.Extensions;

internal static class PropertySymbolExtensions
{
    public static bool IsAssignable(this IPropertySymbol property) =>
        property.SetMethod is not null && property.SetMethod.DeclaredAccessibility == Accessibility.Public;
    
    public static bool IsSoftInit(this IPropertySymbol property, bool withinStrictType) =>
        (withinStrictType && property.HasSoftInitAttribute()) ||
        (! withinStrictType && ! property.HasStrictInitAttribute());

    public static bool IsStrictInit(this IPropertySymbol property, bool withinStrictType) =>
        (withinStrictType && ! property.HasSoftInitAttribute()) ||
        (! withinStrictType && property.HasStrictInitAttribute());

    public static bool HasStrictInitAttribute(this IPropertySymbol property) =>
        property.GetAttributes().Any(attr => attr.IsStrictInitAttribute());

    public static bool HasSoftInitAttribute(this IPropertySymbol property) =>
        property.GetAttributes().Any(attr => attr.IsSoftInitAttribute());
}