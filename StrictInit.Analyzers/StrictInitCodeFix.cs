using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using StrictInit.Analyzers.Diagnostics;
using StrictInit.Analyzers.Extensions;

namespace StrictInit.Analyzers;

public class StrictInitCodeFix : CodeFixProvider
{
    private const string TITLE = "Initialize missing properties with default values";
    
    public override ImmutableArray<string> FixableDiagnosticIds { get; } 
        = ImmutableArray.Create(SetAllProperties.Id, SetAllStrictInitProperties.Id);

    public override FixAllProvider? GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (root is null || semanticModel is null)
            return;
        
        var diagnostics = context.Diagnostics.First();
        bool includeSoftProperties = diagnostics.Id == SetAllProperties.Id;
        var initializerCode = diagnostics.Location.SourceSpan;
        var initializer = root.FindToken(initializerCode.Start).Parent?.AncestorsAndSelf()
            .OfType<InitializerExpressionSyntax>().FirstOrDefault();
        if (initializer is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                TITLE, 
                cancelToken => Fix(context.Document, initializer, semanticModel, includeSoftProperties, cancelToken),
                TITLE), 
            diagnostics);
    }

    private async Task<Document> Fix(Document document, InitializerExpressionSyntax objectInitializer, SemanticModel semanticModel, bool includeSoftProperties, CancellationToken cancelToken)
    {
        var constructedType = objectInitializer.GetConstructedSymbol(semanticModel);
        if (constructedType is null)
            return document;

        var properties = GetUnassignedProperties(objectInitializer, constructedType, includeSoftProperties);

        var expressions = objectInitializer.Expressions;
        foreach (var property in properties)
        {
            expressions = expressions.Add(SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(property),
                SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression))
            );
        }

        var newInit = objectInitializer
            .WithExpressions(expressions)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var oldRoot = await document.GetSyntaxRootAsync(cancelToken);
        if (oldRoot is null)
            return document;
        var newRoot = oldRoot.ReplaceNode(objectInitializer, newInit);

        return document.WithSyntaxRoot(newRoot);
    }

    private IEnumerable<string> GetUnassignedProperties(
        InitializerExpressionSyntax objectInitializer,
        INamedTypeSymbol constructedType,
        bool includeSoftProperties)
    {
        var assignedProperties = objectInitializer.GetAssignedPropertiesNames();
        var unassignedProperties = constructedType.GetUnassignedStrictProperties(assignedProperties);

        return includeSoftProperties
            ? unassignedProperties.Concat(constructedType.GetUnassignedSoftProperties(assignedProperties))
            : unassignedProperties;
    }

}