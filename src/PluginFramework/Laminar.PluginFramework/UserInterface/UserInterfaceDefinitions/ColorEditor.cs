using System.Drawing;

namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class ColorEditor : IUserInterfaceDefinition
{
    public interface IXamlTarget : IInterfaceData<ColorEditor, Color>;
}