using Laminar.PluginSourceGeneration.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Laminar.PluginSourceGeneration.NodeComponentAttributes;

internal interface INodeComponentAttributeGenerator
{
    public string Name { get; }

    public string AQN { get; }

    public string AttributeSourceString { get; }

    public NodeImplementationGenerator.ComponentGenerationInfo GenerateComponentInfo(FieldDeclarationSyntax fieldDeclaration, AttributeSyntax attribute, GeneratorSyntaxContext context);
}
