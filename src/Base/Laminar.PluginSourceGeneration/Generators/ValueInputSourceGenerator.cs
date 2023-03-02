using System.Linq;
using System.Text;
using Laminar.PluginSourceGeneration.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Laminar.PluginSourceGeneration.Generators;

public class ValueInputSourceGenerator
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
            predicate: static (node, _) => AttributeSyntaxHelpers.IsFieldSyntaxWithAttributeCalled(node, "Input"),
            transform: static (genContext, _) => AttributeSyntaxHelpers.GetFieldWithInputAttributeNode(genContext, "Laminar.PluginFramework.NodeSystem.Attributes.InputAttribute"))
            .Where(static m => m is not (null, null));

        context.RegisterSourceOutput(fieldsWithInputAttribute, (sgc, fieldAndAttribute) =>
        {
            ClassDeclarationSyntax parentClassSyntax = (ClassDeclarationSyntax)fieldAndAttribute.field.Parent;

            if (!SyntaxHelpers.SyntaxTokenListContains(parentClassSyntax.Modifiers, "partial"))
            {
                sgc.ReportDiagnostic(Diagnostic.Create(LaminarDiagnostics.ObjectNotPartialError, parentClassSyntax.Identifier.GetLocation(), "\"Input\"", SyntaxHelpers.GetLineOf(fieldAndAttribute.field)));
                return;
            }

            if (parentClassSyntax.Parent is not BaseNamespaceDeclarationSyntax)
            {
                sgc.ReportDiagnostic(Diagnostic.Create(LaminarDiagnostics.ClassIsSubclassError, parentClassSyntax.Identifier.GetLocation(), "\"Input\"", SyntaxHelpers.GetLineOf(fieldAndAttribute.field)));
                return;
            }

            sgc.AddSource($"{parentClassSyntax.Identifier.Text}.{fieldAndAttribute.field.Declaration.Variables[0].Identifier.Text}.g.cs", GenerateInputPropertySyntaxNode(parentClassSyntax, fieldAndAttribute.field, fieldAndAttribute.attribute).GetText(Encoding.UTF8));

            sgc.AddSource($"{parentClassSyntax.Identifier.Text}.{fieldAndAttribute.field.Declaration.Variables[0].Identifier.Text}.Registration.g.cs", NodeComponentHelpers.GenerateNodeComponentRegistration(parentClassSyntax, fieldAndAttribute.field, fieldAndAttribute.field.Declaration.Variables[0].Identifier.Text + "NodeRow").GetText(Encoding.UTF8));
        });
    }

    private static SyntaxNode GenerateInputPropertySyntaxNode(ClassDeclarationSyntax parentClass, FieldDeclarationSyntax fieldNeedingInput, AttributeSyntax inputAttribute)
    {
        CompilationUnitSyntax originalRoot = parentClass.SyntaxTree.GetCompilationUnitRoot();

        string fieldNeedingInputName = fieldNeedingInput.Declaration.Variables[0].Identifier.Text;

        string variableType = fieldNeedingInput.Declaration.Type.ToString();

        string initialValue = fieldNeedingInput.Declaration.Variables[0].Initializer.Value.ToString();

        CompilationUnitSyntax newCompilationUnit = SyntaxFactory.CompilationUnit(originalRoot.Externs, originalRoot.Usings, originalRoot.AttributeLists, new SyntaxList<MemberDeclarationSyntax>(
            (parentClass.Parent as BaseNamespaceDeclarationSyntax)
                .RemoveNodes(parentClass.Parent.ChildNodes().Where(x => x is not NameSyntax), SyntaxRemoveOptions.KeepNoTrivia)
                .AddMembers(SyntaxFactory.ParseMemberDeclaration($@"
{parentClass.Modifiers} class {parentClass.Identifier.ToFullString()}
{{
    readonly ValueInputRow<{variableType}> {fieldNeedingInputName}NodeRow = Laminar.PluginFramework.LaminarFactory.Component.ValueInput({inputAttribute.ArgumentList.Arguments[0].GetText()}, {initialValue}, valueAutoSetter: {fieldNeedingInputName}ValueSetter);

    private void {fieldNeedingInputName}ValueSetter({variableType} value) 
    {{
        {fieldNeedingInputName} = value;
    }}
}}
"))));
        return newCompilationUnit;
    }
}
