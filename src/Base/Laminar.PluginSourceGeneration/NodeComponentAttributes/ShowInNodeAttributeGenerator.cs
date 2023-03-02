using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Laminar.PluginSourceGeneration.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Laminar.PluginSourceGeneration.NodeComponentAttributes;

internal class ShowInNodeAttributeGenerator : INodeComponentAttributeGenerator
{
    public string Name { get; } = "ShowInNode";

    public string AQN { get; } = $"Laminar.PluginFramework.NodeSystem.Attributes.ShowInNodeAttribute";

    public string AttributeSourceString { get; } = @"
using System;

namespace Laminar.PluginFramework.NodeSystem.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class ShowInNodeAttribute : Attribute
    {
    }
}";

    public NodeImplementationGenerator.ComponentGenerationInfo GenerateComponentInfo(FieldDeclarationSyntax fieldDeclaration, AttributeSyntax attribute, GeneratorSyntaxContext context)
    {
        return new NodeImplementationGenerator.ComponentGenerationInfo(null, fieldDeclaration.Declaration.Variables[0].Identifier.Text, GetDiagnostics(context, fieldDeclaration));
    }

    private IEnumerable<Diagnostic> GetDiagnostics(GeneratorSyntaxContext context, FieldDeclarationSyntax fieldDeclaration)
    {
        if (!(context.SemanticModel.GetSymbolInfo(fieldDeclaration.Declaration.Type).Symbol as INamedTypeSymbol)!.AllInterfaces.Any(x => x.ToString() == "Laminar.PluginFramework.NodeSystem.Components.INodeComponent"))
        {
            yield return Diagnostic.Create(LaminarDiagnostics.TypeDoesNotInheritError, fieldDeclaration.GetLocation(), Name, "INodeComponent");
        }
    }
}
