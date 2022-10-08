using Microsoft.CodeAnalysis;

namespace StrictInit.Analyzers.Extensions;

internal static class AttributeDataExtensions
{
    public static bool IsStrictInitAttribute(this AttributeData attribute) =>
        attribute.AttributeClass is not null && attribute.AttributeClass.Name == "StrictInitAttribute";

    public static bool IsSoftInitAttribute(this AttributeData attribute) => 
        attribute.AttributeClass is not null && attribute.AttributeClass.Name == "SoftInitAttribute";
}