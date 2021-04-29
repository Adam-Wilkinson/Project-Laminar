using Laminar_PluginFramework.Primitives;
using System;

namespace Laminar_PluginFramework.Registration
{
    /// <summary>
    /// Defines the class whcih registers a plugin with the <see cref="IPluginHost"/>
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// Registers the plugin with the host
        /// </summary>
        /// <param name="host">The plugin host this plugin will be registered with</param>
        void Register(IPluginHost host);

        Platforms Platforms { get; }

        string PluginName { get; }

        string PluginDescription { get; }
    }
}
