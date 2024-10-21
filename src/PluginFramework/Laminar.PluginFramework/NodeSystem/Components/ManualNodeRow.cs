using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class ManualNodeRow<TInput, TDisplay, TOutput> : SingleItemNodeComponent
    where TDisplay : IDisplayValue
{
    internal ManualNodeRow(INodeRow row)
    {
        ChildComponent = row;
    }

    public TInput? Input { get; init; }

    public required TDisplay Display { get; init; }

    public TOutput? Output { get; init; }
}

public static class ManualNodeRowFactoryExtensions
{
    public static ManualNodeRow<TInput, TDisplay, TOutput> ManualNodeRow<TInput, TDisplay, TOutput>(this INodeComponentFactory factory, TInput input, TDisplay displayValue, TOutput output)
        where TInput : IInput
        where TDisplay : IDisplayValue
        where TOutput : IOutput
    {
        return new ManualNodeRow<TInput, TDisplay, TOutput>(factory.CreateSingleRow(input, displayValue, output))
        {
            Input = input,
            Display = displayValue,
            Output = output,
        };
    }

    public static ManualNodeRow<TInput, TDisplay, None> ManualInputRow<TInput, TDisplay>(this INodeComponentFactory factory, TInput input, TDisplay displayValue)
        where TInput : IInput
        where TDisplay : IDisplayValue
    {
        return new ManualNodeRow<TInput, TDisplay, None>(factory.CreateSingleRow(input, displayValue, null))
        {
            Input = input,
            Display = displayValue,
        };
    }

    public static ManualNodeRow<None, TDisplay, TOutput> ManualOutputRow<TDisplay, TOutput>(this INodeComponentFactory factory, TDisplay displayValue, TOutput output)
        where TDisplay : IDisplayValue
        where TOutput : IOutput
    {
        return new ManualNodeRow<None, TDisplay, TOutput>(factory.CreateSingleRow(null, displayValue, output))
        {
            Display = displayValue,
            Output = output,
        };
    }

    public static ManualNodeRow<None, TDisplay, None> ManualDisplayRow<TDisplay>(this INodeComponentFactory factory, TDisplay displayValue)
        where TDisplay : IDisplayValue
    {
        return new ManualNodeRow<None, TDisplay, None>(factory.CreateSingleRow(null, displayValue, null)) 
        { 
            Display = displayValue, 
        };
    }
}
