using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Laminar.PluginSourceGeneration
{
    [Generator(LanguageNames.CSharp)]
    public class ValueInputSourceGenerator : IIncrementalGenerator
    {
        const string InputAttribute = @"
namespace Laminar.PluginFramework.NodeSystem.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    internal class InputAttribute : System.Attribute
    {
        public InputAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(postInitializationContext => postInitializationContext.AddSource("InputAttribute.g.cs", SourceText.From(InputAttribute, Encoding.UTF8)));

            IncrementalValuesProvider<(FieldDeclarationSyntax field, AttributeSyntax attribute)> fieldsWithInputAttribute = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) => IsFieldSyntaxWithAttributeCalledInput(node),
                transform: (genContext, _) => GetFieldWithInputAttributeNode(genContext))
                .Where(m => m.Item1 != null && m.Item2 != null);

            context.RegisterSourceOutput(fieldsWithInputAttribute, (sgc, fieldAndAttribute) =>
            {
                sgc.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("ad05", "This is an input", "Wow how exciting it is an input called " + fieldAndAttribute.attribute.ArgumentList.Arguments.First().ToFullString(), "fancies", DiagnosticSeverity.Warning, true),
                                                        fieldAndAttribute.field.GetLocation()));
            });
        }

        private static bool IsFieldSyntaxWithAttributeCalledInput(SyntaxNode node)
        {
            return node is FieldDeclarationSyntax fieldSyntax && fieldSyntax.AttributeLists.Count > 0 && fieldSyntax.AttributeLists.Any(attributeListSyntax => attributeListSyntax.Attributes.Any(x => GetSimpleName(x.Name) == "Input" && x.ArgumentList.Arguments.Count >= 1));
        }

        private static (FieldDeclarationSyntax, AttributeSyntax) GetFieldWithInputAttributeNode(GeneratorSyntaxContext genContext)
        {
            FieldDeclarationSyntax fieldNode = (FieldDeclarationSyntax)genContext.Node;

            foreach (AttributeListSyntax attributeList in fieldNode.AttributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    if (genContext.SemanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeConstructorSymbol)
                    {
                        INamedTypeSymbol attributeTypeSymbol = attributeConstructorSymbol.ContainingType;

                        if (attributeTypeSymbol.ToDisplayString() == "Laminar.PluginFramework.NodeSystem.Attributes.InputAttribute")
                        {
                            return (fieldNode, attribute);
                        }
                    }
                }
            }

            // Could not find field and correctly typed attribute
            return (null, null);
        }

        private static string GetSimpleName(NameSyntax nameSyntax)
        {
            switch (nameSyntax.Kind())
            {
                case SyntaxKind.IdentifierName:
                case SyntaxKind.GenericName:
                    return ((SimpleNameSyntax)nameSyntax).Identifier.ValueText;
                case SyntaxKind.QualifiedName:
                    return GetSimpleName(((QualifiedNameSyntax)nameSyntax).Right);
                case SyntaxKind.AliasQualifiedName:
                    return ((AliasQualifiedNameSyntax)nameSyntax).Name.Identifier.ValueText;
                default:
                    throw new ArgumentException(nameof(nameSyntax));
            }
        }
    }
}
