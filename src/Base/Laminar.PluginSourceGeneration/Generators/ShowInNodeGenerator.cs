using System.Linq;
using System.Text;
using Laminar.PluginSourceGeneration.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Laminar.PluginSourceGeneration.Generators;

public class ShowInNodeGenerator
{
    const string attributeName = "ShowInNode";

    const string ShowInNodeAttribute = @$"
namespace Laminar.PluginFramework.NodeSystem.Attributes
{{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    internal class {attributeName}Attribute : System.Attribute
    {{
    }}
}}";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(postInitializationContext => postInitializationContext.AddSource($"{attributeName}Attribute.g.cs", SourceText.From(ShowInNodeAttribute, Encoding.UTF8)));

        IncrementalValuesProvider<(FieldDeclarationSyntax field, AttributeSyntax attribute)> fieldsWithInputAttribute = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => AttributeSyntaxHelpers.IsFieldSyntaxWithAttributeCalled(node, attributeName),
            transform: static (genContext, _) => AttributeSyntaxHelpers.GetFieldWithInputAttributeNode(genContext, $"Laminar.PluginFramework.NodeSystem.Attributes.{attributeName}Attribute"))
            .Where(static m => m is not (null, null));

        context.RegisterSourceOutput(fieldsWithInputAttribute, (sgc, fieldAndAttribute) =>
        {
            ClassDeclarationSyntax parentClassSyntax = (ClassDeclarationSyntax)fieldAndAttribute.field.Parent;

            if (!SyntaxHelpers.SyntaxTokenListContains(parentClassSyntax.Modifiers, "partial"))
            {
                sgc.ReportDiagnostic(Diagnostic.Create(LaminarDiagnostics.ObjectNotPartialError, parentClassSyntax.Identifier.GetLocation(), $"\"{attributeName}\"", SyntaxHelpers.GetLineOf(fieldAndAttribute.field)));
                return;
            }

            if (parentClassSyntax.Parent is not BaseNamespaceDeclarationSyntax)
            {
                sgc.ReportDiagnostic(Diagnostic.Create(LaminarDiagnostics.ClassIsSubclassError, parentClassSyntax.Identifier.GetLocation(), $"\"{attributeName}\"", SyntaxHelpers.GetLineOf(fieldAndAttribute.field)));
                return;
            }

            sgc.AddSource($"{parentClassSyntax.Identifier.Text}.{fieldAndAttribute.field.Declaration.Variables[0].Identifier.Text}.Registration.g.cs", NodeComponentHelpers.GenerateNodeComponentRegistration(parentClassSyntax, fieldAndAttribute.field, fieldAndAttribute.field.Declaration.Variables[0].Identifier.Text).GetText(Encoding.UTF8));
        });
    }
}
