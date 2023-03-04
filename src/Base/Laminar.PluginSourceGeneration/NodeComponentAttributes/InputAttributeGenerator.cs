﻿using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.PluginSourceGeneration.Generators;
using Laminar.PluginSourceGeneration.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Laminar.PluginSourceGeneration.NodeComponentAttributes;
internal class InputAttributeGenerator : INodeComponentAttributeGenerator
{
    public string Name { get; } = "Input";

    public string AttributeSourceString(string attributeNamespace) =>
$@"using System;

namespace {attributeNamespace}
{{
    [AttributeUsage(AttributeTargets.Field)]
    internal class {Name}Attribute : Attribute
    {{
        public {Name}Attribute(string inputName) 
        {{
            InputName = inputName;
        }}

        public string InputName {{ get; }}
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

        string initialValue = variableDeclaration.Initializer!.Value.ToString();

        MemberDeclarationSyntax memberDeclaration = SyntaxFactory.ParseMemberDeclaration($"private ValueInputRow<{FieldDeclarationType}> {ComponentName};");

        return new NodeImplementationGenerator.ComponentGenerationInfo(memberDeclaration, $"{ComponentName} ??= Component.ValueInput<{FieldDeclarationType}>({attribute.ArgumentList!.Arguments[0].GetText()}, {initialValue}, valueAutoSetter: (value) => {fieldName} = value)", GetDiagnostics(context, fieldDeclaration)) { IsSuccessful = isSuccessful };
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
