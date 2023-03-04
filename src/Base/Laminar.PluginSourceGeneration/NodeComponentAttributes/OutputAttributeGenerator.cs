using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laminar.PluginSourceGeneration.Generators;
using Laminar.PluginSourceGeneration.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Laminar.PluginSourceGeneration.NodeComponentAttributes;
internal class OutputAttributeGenerator : INodeComponentAttributeGenerator
{
    public string Name { get; } = "Output";

    public string AttributeSourceString(string attributeNamespace) =>
$@"using System;

namespace {attributeNamespace}
{{
    [AttributeUsage(AttributeTargets.Field)]
    internal class {Name}Attribute : Attribute
    {{
        public {Name}Attribute(string outputName) 
        {{
            OutputName = outputName;
        }}

        public string OutputName {{ get; }}
    }}
}}";

    public NodeImplementationGenerator.ComponentGenerationInfo GenerateComponentInfo(FieldDeclarationSyntax fieldDeclaration, AttributeSyntax attribute, GeneratorSyntaxContext context)
    {
        IEnumerable<Diagnostic> diagnostics = GetDiagnostics(context, fieldDeclaration);

        bool isSuccessful = !diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

        VariableDeclaratorSyntax variableDeclaration = fieldDeclaration.Declaration.Variables[0];

        string fieldName = variableDeclaration.Identifier.Text;

        string ComponentName = $"{fieldName}NodeComponent";

        string FieldDeclarationType = fieldDeclaration.Declaration.Type.ToString();

        MemberDeclarationSyntax memberDeclaration = SyntaxFactory.ParseMemberDeclaration($"private ValueOutputRow<{FieldDeclarationType}> {ComponentName};");

        StringBuilder componentBuilder = new();
        componentBuilder.Append($"{ComponentName} ??= Component.ValueOutput<{FieldDeclarationType}>({attribute.ArgumentList!.Arguments[0].GetText()}");

        if (variableDeclaration.Initializer is not null)
        {
            componentBuilder.Append($", initialValue: {variableDeclaration.Initializer.Value}");
        }

        componentBuilder.Append($", manualValueGetter: () => {fieldName})");

        return new NodeImplementationGenerator.ComponentGenerationInfo(memberDeclaration, componentBuilder.ToString(), GetDiagnostics(context, fieldDeclaration)) { IsSuccessful = isSuccessful };
    }

    private IEnumerable<Diagnostic> GetDiagnostics(GeneratorSyntaxContext context, FieldDeclarationSyntax fieldDeclaration)
    {
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
