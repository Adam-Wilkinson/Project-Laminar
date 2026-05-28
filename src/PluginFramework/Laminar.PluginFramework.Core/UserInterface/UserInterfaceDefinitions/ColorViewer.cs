using System.Drawing;

namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class ColorViewer : IUserInterfaceDefinition
{
    public interface IXamlTarget : IInterfaceData<ColorViewer, Color>;
}