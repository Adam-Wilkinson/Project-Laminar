namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class Slider : IUserInterfaceDefinition
{
    public static readonly InterfaceData<Slider, double> DesignInstance = new(5.0) { Name = "Default Slider", Definition = new Slider { Min = 0.0, Max = 100.0 }};
    
    public interface IXamlTarget : IInterfaceData<Slider, double>;

    public double Max { get; init; } = 0;

    public double Min { get; init; } = 100;

    public string FormatString { get; init; } = "{0}";

    public double Increment { get; init; }
}