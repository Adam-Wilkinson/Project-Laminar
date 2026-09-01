using System.ComponentModel;

namespace Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

public class EditableLabel : IUserInterfaceDefinition, INotifyPropertyChanged
{
    public static readonly InterfaceData<EditableLabel, string> DesignInstance = new("Default Value") { Name = "Default Name" };
    
    public interface IXamlTarget : IInterfaceData<EditableLabel, string>;

    public bool IsBeingEdited
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBeingEdited)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}