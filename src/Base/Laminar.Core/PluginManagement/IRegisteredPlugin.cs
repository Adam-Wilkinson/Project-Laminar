using Laminar.PluginFramework.NodeSystem;
using System;
using System.Collections.Generic;

namespace Laminar.Core.PluginManagement;

public interface IRegisteredPlugin
{
    string PluginName { get; }

    string PluginDescription { get; }

    public IReadOnlyDictionary<string, Type> RegisteredNodes { get; }

    public bool ContainsNode(INode node);

    void Load();

    void RegisterNode(INode node);

    void Unload();
}
