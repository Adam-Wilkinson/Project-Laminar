using System;
using System.IO;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Avalonia.Threading;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;

namespace Laminar.Avalonia.Settings;

public class SettingsSerializer(TopLevel topLevel, IPersistentDataManager persistentDataManager) : IAfterApplicationBuiltTarget
{
    private readonly IPersistentDictionary _settingsDataStore = persistentDataManager.GetDataStore(DataStoreKey.Settings);
    private bool _initialized;
    
    public void OnApplicationBuilt()
    {
        topLevel.GetResourceObservable("SettingsRoot").Subscribe(new AnonymousObserver<object?>(x =>
        {
            if (x is SettingsCategory category && !_initialized)
            {
                _initialized = true;
                SerializeCategory(category, "");
            }
        }));
    }
    
    private void Serialize(SettingsItem settingsItem, string prefix = "")
    {
        var key = string.IsNullOrEmpty(prefix) ? settingsItem.Name : prefix + "." + settingsItem.Name;
        
        switch (settingsItem)
        {
            case SettingsCategory category:
                SerializeCategory(category, key);
                break;
            case Setting setting:
                SerializeSetting(setting, key);
                break;
        }
    }

    private void SerializeCategory(SettingsCategory category, string settingKey)
    {
        foreach (var childSetting in category)
        {
            Serialize(childSetting, settingKey);
        }
    }

    private void SerializeSetting(Setting setting, string settingKey)
    {
        var newSettingObservable = _settingsDataStore
            .InitializeValue(settingKey, setting.Value, serializationKeyOverride: setting.Value.GetType());

        setting.Value = newSettingObservable.Value;
        
        newSettingObservable.OnChanged += (_, valueChangedArgs) =>
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Invoke(() => setting.Value = valueChangedArgs.NewValue!);
            }
            else
            {
                setting.Value = valueChangedArgs.NewValue!;   
            }
        };

        setting.GetObservable(Setting.ValueProperty).Subscribe(new AnonymousObserver<object>(x =>
            _settingsDataStore.SetValue(settingKey, x)));

        setting.ResetCommand = new ResetSettingCommand(_settingsDataStore, settingKey);
    }
    
    private class ResetSettingCommand : ICommand
    {
        private readonly IObservableValueWithDefault<object> _settingObservable;
        private bool _canExecute;

        public ResetSettingCommand(IPersistentDictionary settingsDataStore, string settingKey)
        {
            if (settingsDataStore.TryGetValue<object>(settingKey) is not { } settingObservable)
                throw new InvalidOperationException($"Unable to find setting {settingKey}");
            
            _settingObservable = settingObservable;
            _settingObservable.OnChanged += (_, _) =>
            {
                bool canNowExecute = !Equals(_settingObservable.Value, _settingObservable.DefaultValue);
                if (_canExecute != canNowExecute)
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        public bool CanExecute(object? parameter)
        {
            _canExecute = !Equals(_settingObservable.Value, _settingObservable.DefaultValue);
            return _canExecute;
        }

        public void Execute(object? parameter) => _settingObservable.Reset();

        public event EventHandler? CanExecuteChanged;
    }
}