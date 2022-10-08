using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using StrictInit.Analyzers.Diagnostics;

namespace StrictInit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StrictInitAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(new[]
        {
            SetAllProperties.Rule,
            SetAllStrictInitProperties.Rule
        });

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ObjectInitializerExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var objectInitializer = (InitializerExpressionSyntax)context.Node;
            var constructedSymbol = GetConstructedSymbol(objectInitializer, context.SemanticModel);
            if (constructedSymbol is null)
                return;

            var assignedProperties = GetAssignedPropertiesNames(objectInitializer);

            var unassignedSoftProperties = GetUnassignedSoftProperties(constructedSymbol, assignedProperties);
            if (unassignedSoftProperties.Count > 0)
            {
                var diagnostic = Diagnostic.Create(
                    SetAllProperties.Rule,
                    objectInitializer.GetLocation(),
                    string.Join(", ", unassignedSoftProperties));
                context.ReportDiagnostic(diagnostic);
            }

            var unassignedStrictProperties = GetUnassignedStrictProperties(constructedSymbol, assignedProperties);
            if (unassignedStrictProperties.Count > 0)
            {
                var diagnostics = Diagnostic.Create(
                    SetAllStrictInitProperties.Rule,
                    objectInitializer.GetLocation(),
                    string.Join(", ", unassignedStrictProperties));
                context.ReportDiagnostic(diagnostics);
            }
        }

        private IList<string> GetUnassignedSoftProperties(INamedTypeSymbol constructedSymbol,
            ISet<string> assignedProperties)
        {
            return GetSoftPropertiesNames(constructedSymbol)
                .Where(softProperty => ! assignedProperties.Contains(softProperty))
                .ToList();
        }

        private IList<string> GetUnassignedStrictProperties(INamedTypeSymbol constructedSymbol,
            ISet<string> assignedProperties)
        {
            return GetStrictPropertiesNames(constructedSymbol)
                .Where(strictProperty => ! assignedProperties.Contains(strictProperty))
                .ToList();
        }

        private HashSet<string> GetAssignedPropertiesNames(InitializerExpressionSyntax objectInitializer)
        {
            var assignedProperties = new HashSet<string>();
            foreach (var expression in objectInitializer.Expressions)
            {
                if (expression is AssignmentExpressionSyntax { Left: IdentifierNameSyntax identifier })
                    assignedProperties.Add(identifier.ToString());
            }

            return assignedProperties;
        }

        private ImmutableArray<string> GetSoftPropertiesNames(INamedTypeSymbol constructedSymbol)
        {
            bool isStrictInitType = HasStrictInitAttribute(constructedSymbol);

            return constructedSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(property => IsAssignableProperty(property) &&
                                   IsSoftInitProperty(property, isStrictInitType)
                )
                .Select(property => property.Name)
                .ToImmutableArray();
        }

        private ImmutableArray<string> GetStrictPropertiesNames(INamedTypeSymbol constructedSymbol)
        {
            bool isStrictInitType = HasStrictInitAttribute(constructedSymbol);

            return constructedSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(property => IsAssignableProperty(property) &&
                                   IsStrictInitProperty(property, isStrictInitType)
                )
                .Select(property => property.Name)
                .ToImmutableArray();
        }

        private bool IsAssignableProperty(IPropertySymbol property) =>
            property.SetMethod is not null && property.SetMethod.DeclaredAccessibility == Accessibility.Public;

        private bool IsSoftInitProperty(IPropertySymbol property, bool withinStrictInitType) =>
            (withinStrictInitType && HasSoftInitAttribute(property)) ||
            (! withinStrictInitType && ! HasStrictInitAttribute(property));

        private bool IsStrictInitProperty(IPropertySymbol property, bool withinStrictInitType) =>
            (withinStrictInitType && ! HasSoftInitAttribute(property)) ||
            (! withinStrictInitType && HasStrictInitAttribute(property));

        private bool HasStrictInitAttribute(INamedTypeSymbol constructedSymbol) =>
            constructedSymbol.GetAttributes().Any(IsStrictInitAttribute);

        private bool HasStrictInitAttribute(IPropertySymbol propertySymbol) =>
            propertySymbol.GetAttributes().Any(IsStrictInitAttribute);

        private bool HasSoftInitAttribute(IPropertySymbol propertySymbol) =>
            propertySymbol.GetAttributes().Any(IsSoftInitAttribute);

        private bool IsStrictInitAttribute(AttributeData attribute) =>
            attribute.AttributeClass is not null && attribute.AttributeClass.Name == "StrictInitAttribute";

        private bool IsSoftInitAttribute(AttributeData attribute) =>
            attribute.AttributeClass is not null && attribute.AttributeClass.Name == "SoftInitAttribute";

        private INamedTypeSymbol? GetConstructedSymbol(InitializerExpressionSyntax objectInitializer,
            SemanticModel semanticModel)
        {
            var constructedType = (objectInitializer.Parent as ObjectCreationExpressionSyntax)?.Type;
            if (constructedType is null)
                return null;

            return semanticModel.GetSymbolInfo(constructedType).Symbol as INamedTypeSymbol;
        }
    }
}