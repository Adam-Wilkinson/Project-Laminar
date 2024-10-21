using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Laminar.PluginSourceGeneration.Generators;
using Laminar.PluginSourceGeneration.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Laminar.PluginSourceGeneration.NodeComponentAttributes;

internal class ShowInNodeAttributeGenerator : INodeComponentAttributeGenerator
{
    public string Name => "ShowInNode";

    public string AttributeSourceString(string attributeNamespace) => 
@$"using System;

namespace {attributeNamespace}
{{
    [AttributeUsage(AttributeTargets.Field)]
    internal class {Name}Attribute : Attribute
    {{
    }}
}}";

    public NodeImplementationGenerator.ComponentGenerationInfo GenerateComponentInfo(FieldDeclarationSyntax fieldDeclaration, AttributeSyntax attribute, GeneratorSyntaxContext context)
    {
        IEnumerable<Diagnostic> diagnostics = GetDiagnostics(context, fieldDeclaration);

        bool isSuccessful = !diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

        return new NodeImplementationGenerator.ComponentGenerationInfo(null, fieldDeclaration.Declaration.Variables[0].Identifier.Text, GetDiagnostics(context, fieldDeclaration)) { IsSuccessful = isSuccessful };
    }

    private IEnumerable<Diagnostic> GetDiagnostics(GeneratorSyntaxContext context, FieldDeclarationSyntax fieldDeclaration)
    {
        if (!(context.SemanticModel.GetSymbolInfo(fieldDeclaration.Declaration.Type).Symbol as INamedTypeSymbol)!.AllInterfaces.Any(x => x.ToString() == "Laminar.PluginFramework.NodeSystem.Components.INodeComponent"))
        {
            yield return LaminarDiagnostics.TypeDoesNotInheritError(Name, "INodeComponent", fieldDeclaration.GetLocation());
        }

        ClassDeclarationSyntax parentClassSyntax = (ClassDeclarationSyntax)fieldDeclaration.Parent;

        if (!SyntaxHelpers.SyntaxTokenListContains(parentClassSyntax!.Modifiers, "partial"))
        {
            yield return LaminarDiagnostics.ObjectNotPartialError($"\"{Name}\"", SyntaxHelpers.GetLineOf(fieldDeclaration), parentClassSyntax.Identifier.GetLocation());
        }

        if (parentClassSyntax.Parent is not BaseNamespaceDeclarationSyntax)
        {
            yield return LaminarDiagnostics.ClassIsSubclassError($"\"{Name}\"", SyntaxHelpers.GetLineOf(fieldDeclaration), parentClassSyntax.Identifier.GetLocation());
        }
    }
}
