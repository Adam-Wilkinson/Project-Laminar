using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Laminar.PluginSourceGeneration.Helpers;

internal static class NodeComponentHelpers
{
    public static SyntaxNode GenerateNodeComponentRegistration(ClassDeclarationSyntax parentClass, FieldDeclarationSyntax positionalField, string nodeComponentName)
    {
        int fieldIndex = SyntaxHelpers.GetIndexInParent(positionalField, node => node is FieldDeclarationSyntax);

        CompilationUnitSyntax originalRoot = parentClass.SyntaxTree.GetCompilationUnitRoot();

        CompilationUnitSyntax newCompilationUnit = SyntaxFactory.CompilationUnit(originalRoot.Externs, originalRoot.Usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(" System.Reflection"))), originalRoot.AttributeLists, new SyntaxList<MemberDeclarationSyntax>(
            (parentClass.Parent as BaseNamespaceDeclarationSyntax)
                .RemoveNodes(parentClass.Parent.ChildNodes().Where(static x => x is not NameSyntax), SyntaxRemoveOptions.KeepNoTrivia)
                .AddMembers(SyntaxFactory.ParseMemberDeclaration($@"
{parentClass.Modifiers} class {parentClass.Identifier.ToFullString()}
{{
    static FieldInfo RegisterField{fieldIndex} = NodeComponentFinder.RegisterNodeComponentAtPosition(typeof({parentClass.Identifier}), {fieldIndex}, nameof({nodeComponentName}));
}}
"))));

        return newCompilationUnit;
    }
}
