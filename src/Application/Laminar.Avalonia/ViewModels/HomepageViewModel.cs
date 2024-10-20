using System.Collections;
using System.Collections.ObjectModel;
using Laminar.Contracts.Scripting;
using Laminar.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels;

public class HomepageViewModel : ViewModelBase
{
    public void AddScriptDuplicate(IScript scriptToClone)
    {

    }

    public ObservableCollection<IScript> AllScripts { get; init; }
}
