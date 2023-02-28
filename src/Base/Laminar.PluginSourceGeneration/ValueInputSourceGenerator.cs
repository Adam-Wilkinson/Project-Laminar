using System;
using System.Collections.Generic;
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

        readonly DiagnosticDescriptor ObjectNotPartialError = new("PL001", "Partial Error", "Use of the attribute {0} on line {1} requires this to be marked as partial for source generation", "Usage", DiagnosticSeverity.Error, true);
        readonly DiagnosticDescriptor ClassIsSubclassError = new("PL001", "Subclass Error", "Use of the attribute {0} on line {1} requires this class to not be defined as a subclass", "Usage", DiagnosticSeverity.Error, true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(postInitializationContext => postInitializationContext.AddSource("InputAttribute.g.cs", SourceText.From(InputAttribute, Encoding.UTF8)));

            IncrementalValuesProvider<(FieldDeclarationSyntax field, AttributeSyntax attribute)> fieldsWithInputAttribute = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => IsFieldSyntaxWithAttributeCalledInput(node),
                transform: static (genContext, _) => GetFieldWithInputAttributeNode(genContext))
                .Where(static m => m is not (null, null));

            context.RegisterSourceOutput(fieldsWithInputAttribute, (sgc, fieldAndAttribute) =>
            {
                ClassDeclarationSyntax parentClassSyntax = (ClassDeclarationSyntax)fieldAndAttribute.field.Parent;

                if (!SyntaxTokenListContainsPartial(parentClassSyntax.Modifiers))
                {
                    sgc.ReportDiagnostic(Diagnostic.Create(ObjectNotPartialError, parentClassSyntax.Identifier.GetLocation(), "\"Input\"", GetLineOf(fieldAndAttribute.field)));
                    return;
                }

                if (parentClassSyntax.Parent is not BaseNamespaceDeclarationSyntax)
                {
                    sgc.ReportDiagnostic(Diagnostic.Create(ClassIsSubclassError, parentClassSyntax.Identifier.GetLocation(), "\"Input\"", GetLineOf(fieldAndAttribute.field)));
                    return;
                }

                sgc.AddSource($"{parentClassSyntax.Identifier.Text}.{fieldAndAttribute.field.Declaration.Variables[0].Identifier.Text}.g.cs", GenerateInputPropertySyntaxNode(parentClassSyntax, fieldAndAttribute.field, fieldAndAttribute.attribute).GetText(Encoding.UTF8));
            });
        }

        private static bool IsFieldSyntaxWithAttributeCalledInput(SyntaxNode node)
        {
            return node is FieldDeclarationSyntax fieldSyntax && fieldSyntax.AttributeLists.Count > 0 && fieldSyntax.AttributeLists.Any(attributeListSyntax => attributeListSyntax.Attributes.Any(x => GetSimpleName(x.Name) == "Input" && x.ArgumentList.Arguments.Count >= 1));
        }

        private static (FieldDeclarationSyntax, AttributeSyntax) GetFieldWithInputAttributeNode(GeneratorSyntaxContext genContext)
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

        private static bool SyntaxTokenListContainsPartial(SyntaxTokenList syntaxTokens)
        {
            foreach (SyntaxToken token in syntaxTokens)
            {
                if (token.Text == "partial")
                {
                    return true;
                }
            }

            return false;
        }

        private static SyntaxNode GenerateInputPropertySyntaxNode(ClassDeclarationSyntax parentClass, FieldDeclarationSyntax fieldNeedingProperty, AttributeSyntax inputAttribute)
        {
            CompilationUnitSyntax originalRoot = parentClass.SyntaxTree.GetCompilationUnitRoot();

            int fieldIndex = 0;
            foreach (SyntaxNode node in parentClass.ChildNodes())
            {
                if (node == fieldNeedingProperty)
                {
                    break;
                }

                if (node is FieldDeclarationSyntax fieldDeclarationSyntax)
                {
                    fieldIndex++;
                }
            }

            CompilationUnitSyntax newCompilationUnit = SyntaxFactory.CompilationUnit(originalRoot.Externs, originalRoot.Usings, originalRoot.AttributeLists, new SyntaxList<MemberDeclarationSyntax>(
                (parentClass.Parent as BaseNamespaceDeclarationSyntax)
                    .RemoveNodes(parentClass.Parent.ChildNodes().Where(x => x is not NameSyntax), SyntaxRemoveOptions.KeepNoTrivia)
                    .AddMembers(SyntaxFactory.ParseMemberDeclaration($@"
{parentClass.Modifiers} class {parentClass.Identifier.ToFullString()}
{{
    public string NameOfInput{fieldNeedingProperty.Declaration.Variables[0].Identifier.Text}()
    {{
        return {inputAttribute.ArgumentList.Arguments[0].GetText()} + "" is at field position {fieldIndex} and has value "" + {fieldNeedingProperty.Declaration.Variables[0].Identifier.Text}.ToString();
    }}
}}
"))));
            return newCompilationUnit;
        }

        private static int GetLineOf(SyntaxNode node)
        {
            return node.SyntaxTree.GetLineSpan(node.GetLocation().SourceSpan).EndLinePosition.Line + 1;
        }
    }
}
