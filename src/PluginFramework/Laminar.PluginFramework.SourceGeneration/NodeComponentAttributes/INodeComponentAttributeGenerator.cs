using Laminar.PluginFramework.SourceGeneration.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Laminar.PluginFramework.SourceGeneration.NodeComponentAttributes;

internal interface INodeComponentAttributeGenerator
{
    public string Name { get; }

    public string AttributeSourceString(string attributeNamespace);

    public NodeImplementationGenerator.ComponentGenerationInfo GenerateComponentInfo(FieldDeclarationSyntax fieldDeclaration, AttributeSyntax attribute, GeneratorSyntaxContext context);
}
