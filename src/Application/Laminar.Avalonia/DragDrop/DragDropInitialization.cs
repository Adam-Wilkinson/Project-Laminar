using System;
using Avalonia.Controls;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Avalonia.Settings;

namespace Laminar.Avalonia.DragDrop;

public class DragDropInitialization(TopLevel topLevel) : IAfterApplicationBuiltTarget
{
    public void OnApplicationBuilt()
    {
        Setting<TimeSpan>.OnChange(topLevel, "SettingsRoot.InterfaceSettings.AnimationDuration", duration =>
        {
            if (DragDrop.AnimateHomeAnimationDuration != duration)
            {
                DragDrop.AnimateHomeAnimationDuration = duration;
            }
        });
    }
}