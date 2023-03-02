using System;
using System.Collections.Generic;
using System.Reflection;
using Laminar.PluginFramework.NodeSystem.Components;

namespace Laminar.PluginFramework.NodeSystem;

public static class NodeComponentFinder
{
    private static readonly Dictionary<Type, FieldInfo?[]> AllNodeFields = new();

    public static FieldInfo RegisterNodeComponentAtPosition(Type nodeType, int position, string fieldName)
    {
        if (!AllNodeFields.ContainsKey(nodeType))
        {
            AllNodeFields.Add(nodeType, new FieldInfo[position + 1]);
        }

        if (AllNodeFields[nodeType].Length < position)
        {
            AllNodeFields[nodeType] = new FieldInfo[position + 1];
        }

        AllNodeFields[nodeType][position] = nodeType.GetField(fieldName);

        return AllNodeFields[nodeType][position]!;
    }

    public static IEnumerable<INodeComponent> GetComponentsFor(object objWhichNeedsFields)
    {
        if (!AllNodeFields.ContainsKey(objWhichNeedsFields.GetType()))
        {
            yield break;
        }

        foreach (FieldInfo fieldInfo in AllNodeFields[objWhichNeedsFields.GetType()])
        {
            if (fieldInfo is not null)
            {
                yield return (INodeComponent)fieldInfo.GetValue(objWhichNeedsFields)!;
            }
        }
    }
}