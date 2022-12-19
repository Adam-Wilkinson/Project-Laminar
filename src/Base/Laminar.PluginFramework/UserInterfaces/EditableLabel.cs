using System.ComponentModel;

namespace Laminar_PluginFramework.UserInterfaces;

public class EditableLabel : IUserInterfaceDefinition, INotifyPropertyChanged
{
    private bool _isBeingEdited;

    public bool IsBeingEdited
    {
        get => _isBeingEdited;
        set
        {
            if (_isBeingEdited != value)
            {
                _isBeingEdited = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBeingEdited)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
