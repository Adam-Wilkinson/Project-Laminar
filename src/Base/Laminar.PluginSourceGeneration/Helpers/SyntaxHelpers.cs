using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Laminar.PluginSourceGeneration.Helpers;
public static class SyntaxHelpers
{
    public static int GetIndexInParent(SyntaxNode node, Func<SyntaxNode, bool> predicate)
    {
        int index = 0;
        foreach (SyntaxNode currentNode in node.Parent.ChildNodes())
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

    public static string GetSimpleName(NameSyntax nameSyntax) => nameSyntax.Kind() switch
    {
        SyntaxKind.IdentifierName or SyntaxKind.GenericName => ((SimpleNameSyntax)nameSyntax).Identifier.ValueText,
        SyntaxKind.QualifiedName => GetSimpleName(((QualifiedNameSyntax)nameSyntax).Right),
        SyntaxKind.AliasQualifiedName => ((AliasQualifiedNameSyntax)nameSyntax).Name.Identifier.ValueText,
        _ => ""
    };
}
