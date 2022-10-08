using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using StrictInit.Analyzers.Diagnostics;
using StrictInit.Analyzers.Extensions;

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
            var constructedSymbol = objectInitializer.GetConstructedSymbol(context.SemanticModel);
            if (constructedSymbol is null)
                return;

            var assignedProperties = objectInitializer.GetAssignedPropertiesNames();

            var unassignedSoftProperties = constructedSymbol.GetUnassignedSoftProperties(assignedProperties);
            if (unassignedSoftProperties.Length > 0)
            {
                var diagnostic = Diagnostic.Create(
                    SetAllProperties.Rule,
                    objectInitializer.GetLocation(),
                    string.Join(", ", unassignedSoftProperties));
                context.ReportDiagnostic(diagnostic);
            }

            var unassignedStrictProperties = constructedSymbol.GetUnassignedStrictProperties(assignedProperties);
            if (unassignedStrictProperties.Length > 0)
            {
                var diagnostics = Diagnostic.Create(
                    SetAllStrictInitProperties.Rule,
                    objectInitializer.GetLocation(),
                    string.Join(", ", unassignedStrictProperties));
                context.ReportDiagnostic(diagnostics);
            }
        }
    }
}