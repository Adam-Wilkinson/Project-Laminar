using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Laminar.PluginSourceGeneration.Helpers;
public static class SyntaxHelpers
{
    public static int GetIndexInParent(SyntaxNode node, Func<SyntaxNode, bool> predicate)
    {
        int index = 0;
        foreach (SyntaxNode currentNode in node.Parent!.ChildNodes())
        {
            if (currentNode == node)
            {
                return index;
            }

            if (predicate(currentNode))
            {
                index++;
            }
        }

        return -1;
    }

    public static int GetLineOf(SyntaxNode node) => node.SyntaxTree.GetLineSpan(node.GetLocation().SourceSpan).EndLinePosition.Line + 1;

    public static bool SyntaxTokenListContains(SyntaxTokenList tokens, string searchString)
    {
        foreach (SyntaxToken token in tokens)
        {
            if (token.Text == searchString)
            {
                return true;
            }
        }

        return false;
    }

    public static SyntaxList<UsingDirectiveSyntax> EnsureUsingsContains(SyntaxList<UsingDirectiveSyntax> currentUsingDirectives, string[] requiredDirectives)
    {
        List<string> remainingRequiredDirectives = new(requiredDirectives);

        foreach (UsingDirectiveSyntax usingDirectiveSyntax in currentUsingDirectives)
        {
            remainingRequiredDirectives.Remove(usingDirectiveSyntax.Name.ToString());
        }

        foreach (string requiredDirective in remainingRequiredDirectives)
        {
            currentUsingDirectives = currentUsingDirectives.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($" {requiredDirective}")));
        }

        return currentUsingDirectives;
    }

    public static SyntaxList<UsingDirectiveSyntax> EnsureUsingsContains(SyntaxList<UsingDirectiveSyntax> currentUsingDirectives, UsingDirectiveSyntax[] requiredDirectives)
    {
        foreach (UsingDirectiveSyntax currentRequiredDirective in requiredDirectives)
        {
            currentUsingDirectives = EnsureUsingsContains(currentUsingDirectives, currentRequiredDirective);
        }

        return currentUsingDirectives;
    }

    public static string GetSimpleName(NameSyntax nameSyntax) => nameSyntax.Kind() switch
    {
        SyntaxKind.IdentifierName or SyntaxKind.GenericName => ((SimpleNameSyntax)nameSyntax).Identifier.ValueText,
        SyntaxKind.QualifiedName => GetSimpleName(((QualifiedNameSyntax)nameSyntax).Right),
        SyntaxKind.AliasQualifiedName => ((AliasQualifiedNameSyntax)nameSyntax).Name.Identifier.ValueText,
        _ => ""
    };

    private static SyntaxList<UsingDirectiveSyntax> EnsureUsingsContains(SyntaxList<UsingDirectiveSyntax> existingDirectives, UsingDirectiveSyntax requiredDirective)
    {
        foreach (UsingDirectiveSyntax directive in existingDirectives)
        {
            if (directive.ToString() == requiredDirective.ToString())
            {
                return existingDirectives;
            }
        }

        return existingDirectives.Add(requiredDirective);
    }
}
