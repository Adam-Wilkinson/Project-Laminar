using Avalonia.Input;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Domain;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Avalonia.Controls;

public class DataInterfaceRegistration(IDataInterfaceFactory interfaceFactory, ITypeInfoStore typeInfoStore) : IAfterApplicationBuiltTarget
{
    public void OnApplicationBuilt()
    {
        typeInfoStore.RegisterType(typeof(KeyGesture),
            new TypeInfo("Key Gesture", new PluginFramework.UserInterface.UserInterfaceDefinitions.KeyGestureEditor(), new StringEditor(), "#FF5533", null));
        interfaceFactory.RegisterInterface<PluginFramework.UserInterface.UserInterfaceDefinitions.KeyGestureEditor, KeyGesture, KeyGestureEditor>();
    }
}