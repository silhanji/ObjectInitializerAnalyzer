using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StrictInit.Analyzers.Extensions;

internal static class InitializerExpressionSyntaxExtensions
{
    public static ImmutableHashSet<string> GetAssignedPropertiesNames(
        this InitializerExpressionSyntax objectInitializer)
    {
        var assignedProperties = ImmutableHashSet.CreateBuilder<string>();
        foreach (var expression in objectInitializer.Expressions)
        {
            if (expression is AssignmentExpressionSyntax { Left: IdentifierNameSyntax identifier })
                assignedProperties.Add(identifier.ToString());
        }

        return assignedProperties.ToImmutable();
    }
    
    public static INamedTypeSymbol? GetConstructedSymbol(
        this InitializerExpressionSyntax objectInitializer,
        SemanticModel semanticModel)
    {
        var constructedType = (objectInitializer.Parent as ObjectCreationExpressionSyntax)?.Type;
        if (constructedType is null)
            return null;

        return semanticModel.GetSymbolInfo(constructedType).Symbol as INamedTypeSymbol;
    }
}