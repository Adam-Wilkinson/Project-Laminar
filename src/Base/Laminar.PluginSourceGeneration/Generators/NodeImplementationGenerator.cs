using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Laminar.PluginSourceGeneration.Helpers;
using Laminar.PluginSourceGeneration.NodeComponentAttributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Laminar.PluginSourceGeneration.Generators;

[Generator(LanguageNames.CSharp)]
public class NodeImplementationGenerator : IIncrementalGenerator
{
    private static readonly UsingDirectiveSyntax[] RequiredUsings =
    {
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(" System.Collections.Generic")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(" Laminar.PluginFramework.NodeSystem.Components")),
        SyntaxFactory.UsingDirective(SyntaxFactory.ParseToken(" static"), null, SyntaxFactory.ParseName(" Laminar.PluginFramework.LaminarFactory")),
    };

    private const string AttributeNamespace = "Laminar.PluginFramework.NodeSystem.Attributes";

    private static readonly Dictionary<string, INodeComponentAttributeGenerator> componentAttributeGenerators = 
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => typeof(INodeComponentAttributeGenerator).IsAssignableFrom(x) && x.IsClass)
            .Select(x => (INodeComponentAttributeGenerator)Activator.CreateInstance(x))
            .ToDictionary(x => $"{AttributeNamespace}.{x.Name}Attribute");

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(postInitializationContext =>
        {
            foreach (INodeComponentAttributeGenerator attributeGenerator in componentAttributeGenerators.Values)
            {
                postInitializationContext.AddSource($"{attributeGenerator.Name}.g.cs", SourceText.From(attributeGenerator.AttributeSourceString(AttributeNamespace), Encoding.UTF8));
            }
        });

        IncrementalValuesProvider<NodeGenerationInfo> componentsOfNodeClassPipeline = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => SyntaxIsProbablyINodeImplementation(node),
            static (genContext, _) => CreateGenerationInfo(genContext))
            .Where(static m => m is not null)!;

        context.RegisterSourceOutput(componentsOfNodeClassPipeline, (spc, nodeGenerationInfo) =>
        {
            foreach (ComponentGenerationInfo componentInfo in nodeGenerationInfo.Components)
            {
                foreach (Diagnostic diagnostic in componentInfo.Diagnostics)
                {
                    spc.ReportDiagnostic(diagnostic);
                }
            }

            spc.AddSource($"{nodeGenerationInfo.ClassDeclaration.Identifier}.g.cs", GenerateNewClassNode(nodeGenerationInfo).GetText(Encoding.UTF8));
        });
    }

    private static SyntaxNode GenerateNewClassNode(NodeGenerationInfo generationInfo)
    {
        CompilationUnitSyntax originalRoot = generationInfo.ClassDeclaration.SyntaxTree.GetCompilationUnitRoot();

        BaseNamespaceDeclarationSyntax baseNamespaceDeclaration = (BaseNamespaceDeclarationSyntax)generationInfo.ClassDeclaration.Parent!;

        baseNamespaceDeclaration = baseNamespaceDeclaration.RemoveNodes(baseNamespaceDeclaration.ChildNodes().Where(static x => x is not NameSyntax), SyntaxRemoveOptions.KeepNoTrivia)!;

        ClassDeclarationSyntax newClassPart = (ClassDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration($@"
{generationInfo.ClassDeclaration.Modifiers} class {generationInfo.ClassDeclaration.Identifier.ToFullString()}
{{
    
}}")!;

        newClassPart = newClassPart.AddMembers(generationInfo.Components.Select(x => x.NewMember).Where(x => x is not null).ToArray()!);

        if (generationInfo.NeedsFields)
        {
            StringBuilder newMemberSB = new();
            newMemberSB.Append(@"
public IEnumerable<INodeComponent> Components
{
    get
    {");
            bool noFields = true;
            foreach (ComponentGenerationInfo componentGenerationInfo in generationInfo.Components)
            {
                if (componentGenerationInfo.IsSuccessful)
                {
                    newMemberSB.AppendLine($"yield return {componentGenerationInfo.ComponentFieldName};");
                    noFields = false;
                }
            }

            if (noFields)
            {
                newMemberSB.AppendLine("yield break;");
            }

            newMemberSB.Append(@"
    }
}");

            newClassPart = newClassPart.AddMembers(SyntaxFactory.ParseMemberDeclaration(newMemberSB.ToString())!);
        }

        baseNamespaceDeclaration = baseNamespaceDeclaration.AddMembers(newClassPart);

        return SyntaxFactory.CompilationUnit(originalRoot.Externs, SyntaxHelpers.EnsureUsingsContains(originalRoot.Usings, RequiredUsings), originalRoot.AttributeLists, new SyntaxList<MemberDeclarationSyntax>(baseNamespaceDeclaration)).NormalizeWhitespace();
    }

    private static bool SyntaxIsProbablyINodeImplementation(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return false;
        }

        if (classDeclarationSyntax.BaseList is not BaseListSyntax baseClasses)
        {
            return false;
        }

        foreach (BaseTypeSyntax baseClass in baseClasses.Types)
        {
            if (baseClass.Type is NameSyntax baseTypeName && SyntaxHelpers.GetSimpleName(baseTypeName) == "INode")
            {
                return true;
            }
        }

        return false;
    }

    private static NodeGenerationInfo? CreateGenerationInfo(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return null;
        }

        if (!ClassDefinitelyImplementsINode(classDeclarationSyntax, context.SemanticModel))
        {
            return null;
        }

        bool needsFieldsAdding = true;

        foreach (MemberDeclarationSyntax member in classDeclarationSyntax.Members)
        {
            if (member is PropertyDeclarationSyntax propertyDeclarationSyntax && propertyDeclarationSyntax.Identifier.Text.ToString() == "Components")
            {
                needsFieldsAdding = false;
                break;
            }
        }

        return new NodeGenerationInfo(classDeclarationSyntax, needsFieldsAdding, GetNodeComponentsOfClass(classDeclarationSyntax, context));
    }

    private static IEnumerable<ComponentGenerationInfo> GetNodeComponentsOfClass(ClassDeclarationSyntax classDeclarationSyntax, GeneratorSyntaxContext generatorSyntaxContext)
    {
        foreach (MemberDeclarationSyntax member in classDeclarationSyntax.Members)
        {
            if (member is not FieldDeclarationSyntax fieldDeclaration)
            {
                continue;
            }

            foreach (AttributeListSyntax attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    if (generatorSyntaxContext.SemanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeConstructorSymbol)
                    {
                        INamedTypeSymbol attributeTypeSymbol = attributeConstructorSymbol.ContainingType;

                        if (componentAttributeGenerators.TryGetValue(attributeTypeSymbol.ToString(), out INodeComponentAttributeGenerator generator))
                        {
                            yield return generator.GenerateComponentInfo(fieldDeclaration, attribute, generatorSyntaxContext);
                        }
                    }
                }
            }
        }
    }

    private static bool ClassDefinitelyImplementsINode(ClassDeclarationSyntax classSyntax, SemanticModel semanticModel)
    {
        foreach (BaseTypeSyntax baseType in classSyntax.BaseList!.Types)
        {
            if (semanticModel.GetSymbolInfo(baseType.Type).Symbol is INamedTypeSymbol typeSymbol)
            {
                if (typeSymbol.ToString() == "Laminar.PluginFramework.NodeSystem.INode")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private record NodeGenerationInfo(ClassDeclarationSyntax ClassDeclaration, bool NeedsFields, IEnumerable<ComponentGenerationInfo> Components);

    public record ComponentGenerationInfo(MemberDeclarationSyntax? NewMember, string ComponentFieldName, IEnumerable<Diagnostic> Diagnostics)
    {
        public bool IsSuccessful { get; set; } = true;
    };
}