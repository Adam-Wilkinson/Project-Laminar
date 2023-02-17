using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        const char DotChar = '.';

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(i => i.AddSource("InputAttribute.g.cs", InputAttribute));

            IncrementalValuesProvider<(FieldDeclarationSyntax field, AttributeSyntax attribute)> inputFields =  context.SyntaxProvider.CreateSyntaxProvider((node, token) => { return node is FieldDeclarationSyntax fieldSyntax && fieldSyntax.AttributeLists.Count > 0 && fieldSyntax.AttributeLists.Any(listSyntax => listSyntax.Attributes.Any(x => BuildName(x.Name, false) == "Input" && x.ArgumentList.Arguments.Count >= 1)); }, (genContext, token) => { return (genContext.Node as FieldDeclarationSyntax, GetInputAttribute(genContext.Node as FieldDeclarationSyntax)); });

            context.RegisterSourceOutput(inputFields, (sgc, fieldAndAttribute) =>
            {
                sgc.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("ad05", "This is an input", "Wow how exciting it is an input called " + fieldAndAttribute.attribute.ArgumentList.Arguments.First().ToFullString(), "fancies", DiagnosticSeverity.Warning, true),
                                                        fieldAndAttribute.field.GetLocation()));
            });
        }



        private static string BuildName(NameSyntax nameSyntax, bool includeAlias)
        {
            if (nameSyntax.IsKind(SyntaxKind.IdentifierName))
            {
                var identifierNameSyntax = (IdentifierNameSyntax)nameSyntax;
                return identifierNameSyntax.Identifier.ValueText;
            }

            if (nameSyntax.IsKind(SyntaxKind.QualifiedName))
            {
                var qualifiedNameSyntax = (QualifiedNameSyntax)nameSyntax;
                return BuildName(qualifiedNameSyntax.Left, includeAlias) + DotChar + BuildName(qualifiedNameSyntax.Right, includeAlias);
            }

            else if (nameSyntax.IsKind(SyntaxKind.GenericName))
            {
                var genericNameSyntax = (GenericNameSyntax)nameSyntax;
                return genericNameSyntax.Identifier.ValueText + genericNameSyntax.TypeArgumentList.ToString();
            }

            else if (nameSyntax.IsKind(SyntaxKind.AliasQualifiedName))
            {
                var aliasQualifiedNameSyntax = (AliasQualifiedNameSyntax)nameSyntax;
                if (includeAlias)
                {
                    return aliasQualifiedNameSyntax.Alias.Identifier.ValueText + "::" + aliasQualifiedNameSyntax.Name.Identifier.ValueText;
                }

                return aliasQualifiedNameSyntax.Name.Identifier.ValueText;
            }

            throw new NotImplementedException();
        }

        private static AttributeSyntax GetInputAttribute(FieldDeclarationSyntax fieldSyntax)
        {
            foreach (var attributeList in fieldSyntax.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (BuildName(attribute.Name, false) == "Input")
                    {
                        return attribute;
                    }
                }
            }

            throw new Exception();
        }
    }
}
