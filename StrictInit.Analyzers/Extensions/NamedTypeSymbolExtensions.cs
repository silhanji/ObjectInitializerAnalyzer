using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace StrictInit.Analyzers.Extensions;

internal static class NamedTypeSymbolExtensions
{
    public static ImmutableArray<string> GetUnassignedStrictProperties(
        this INamedTypeSymbol namedTypeSymbol,
        IImmutableSet<string> assignedProperties)
    {
        var unassigned = namedTypeSymbol.GetStrictPropertiesNames()
            .Where(property => ! assignedProperties.Contains(property))
            .ToArray();
        
        return ImmutableArray.Create(unassigned);
    }

    public static ImmutableArray<string> GetUnassignedSoftProperties(
        this INamedTypeSymbol namedTypeSymbol,
        IImmutableSet<string> assignedProperties)
    {
        var unassigned = namedTypeSymbol.GetSoftPropertiesNames()
            .Where(property => ! assignedProperties.Contains(property))
            .ToArray();
        
        return ImmutableArray.Create(unassigned);
    }

    public static ImmutableArray<string> GetStrictPropertiesNames(this INamedTypeSymbol namedTypeSymbol)
    {
        bool isStrictInitType = HasStrictInitAttribute(namedTypeSymbol);

        return namedTypeSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(property => property.IsAssignable() &&
                               property.IsStrictInit(isStrictInitType)
            )
            .Select(property => property.Name)
            .ToImmutableArray();
    }

    public static ImmutableArray<string> GetSoftPropertiesNames(this INamedTypeSymbol namedTypeSymbol)
    {
        bool isStrictInitType = HasStrictInitAttribute(namedTypeSymbol);

        return namedTypeSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(property => property.IsAssignable() &&
                               property.IsSoftInit(isStrictInitType)
            )
            .Select(property => property.Name)
            .ToImmutableArray();
    }

    private static bool HasStrictInitAttribute(INamedTypeSymbol constructedSymbol) =>
        constructedSymbol.GetAttributes().Any(attr => attr.IsStrictInitAttribute());
}