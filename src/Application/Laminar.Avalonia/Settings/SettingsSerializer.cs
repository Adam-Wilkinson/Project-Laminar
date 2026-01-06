using System;
using System.IO;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Contracts.UserData;
using Laminar.Domain.DataManagement;

namespace Laminar.Avalonia.Settings;

public class SettingsSerializer(TopLevel topLevel, IPersistentDataManager persistentDataManager) : IAfterApplicationBuiltTarget
{
    private readonly IPersistentDataStore _settingsDataStore = persistentDataManager.GetDataStore(DataStoreKey.Settings);
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
        _settingsDataStore.InitializeDefaultValue(settingKey, setting.Value, setting.Value.GetType());
        setting.Value = _settingsDataStore.GetItem(settingKey, setting.Value.GetType()).Result!;
            
        _settingsDataStore.GetObservable(settingKey).ValueChanged += (_, newValue) =>
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Invoke(() => setting.Value = newValue);
            }
            else
            {
                setting.Value = newValue;   
            }
        };

        setting.GetObservable(Setting.ValueProperty).Subscribe(new AnonymousObserver<object>(x =>
            _settingsDataStore.SetItem(settingKey, x)));

        setting.ResetCommand = new ResetSettingCommand(_settingsDataStore, settingKey, setting);
    }
    
    private class ResetSettingCommand : ICommand
    {
        private readonly IPersistentDataStore _settingsDataStore;
        private readonly string _settingKey;
        private readonly Setting _setting;
        private bool _canExecute;

        public ResetSettingCommand(IPersistentDataStore settingsDataStore, string settingKey, Setting setting)
        {
            _settingsDataStore = settingsDataStore;
            _settingKey = settingKey;
            _setting = setting;
            _settingsDataStore.GetObservable(settingKey).ValueChanged += (_, newValue) =>
            {
                var defaultVal = SettingDefaultValue();
                bool canNowExecute = !Equals(newValue, defaultVal);
                if (_canExecute != canNowExecute)
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        public bool CanExecute(object? parameter)
        {
            var defaultVal = SettingDefaultValue();
            _canExecute = !Equals(_setting.Value, defaultVal);
            return _canExecute;
        }

        public void Execute(object? parameter) => _settingsDataStore.ResetToDefault(_settingKey);

        public event EventHandler? CanExecuteChanged;

        private object? SettingDefaultValue()
        {
            var defaultRead = _settingsDataStore.GetDefaultValue(_settingKey);
            if (defaultRead.Status != DataIoStatus.Success)
            {
                throw new IOException("Invalid data read");
            }
            
            return defaultRead.Result;
        }
    }
}