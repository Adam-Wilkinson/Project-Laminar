using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Laminar.PluginSourceGeneration.Helpers;

internal static class AttributeSyntaxHelpers
{
    public static bool IsFieldSyntaxWithAttributeCalled(SyntaxNode node, string name)
    {
        return node is FieldDeclarationSyntax fieldSyntax && fieldSyntax.AttributeLists.Count > 0 && fieldSyntax.AttributeLists.Any(attributeListSyntax => attributeListSyntax.Attributes.Any(x => SyntaxHelpers.GetSimpleName(x.Name) == name));
    }

    public static (FieldDeclarationSyntax, AttributeSyntax) GetFieldWithInputAttributeNode(GeneratorSyntaxContext genContext, string assemblyQualifiedName)
    {
        FieldDeclarationSyntax fieldNode = (FieldDeclarationSyntax)genContext.Node;

        if (fieldNode.Parent is not ClassDeclarationSyntax)
        {
            return (null, null);
        }

        foreach (AttributeListSyntax attributeList in fieldNode.AttributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                if (genContext.SemanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeConstructorSymbol)
                {
                    INamedTypeSymbol attributeTypeSymbol = attributeConstructorSymbol.ContainingType;

                    if (attributeTypeSymbol.ToDisplayString() == assemblyQualifiedName)
                    {
                        return (fieldNode, attribute);
                    }
                }
            }
        }

        // Could not find field and correctly typed attribute
        return (null, null);
    }
}
