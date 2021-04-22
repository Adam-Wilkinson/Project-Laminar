using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Laminar_Core.Primitives.UserInterface
{
    public class UserInterfaceManager : IUserInterfaceManager
    {
        private ILaminarValue _childValue;
        private string _userInterfaceType;
        private readonly IObjectFactory _factory;
        private readonly Instance _instance;
        private readonly List<IObservableValue<object>> _userInterfaces = new();

        public UserInterfaceManager(Instance instance)
        {
            _factory = instance.Factory;
            _instance = instance;
        }

        public object this[string key]
        {
            get
            {
                if (_userInterfaceType == null)
                {
                    _userInterfaceType = key;
                }

                if (_userInterfaceType != key)
                {
                    throw new Exception($"{nameof(UserInterfaceManager)} cannot handle multiple types of user interface at once!");
                }

                IObservableValue<object> newObject = _factory.GetImplementation<IObservableValue<object>>();
                newObject.Value = GetUIOfType(key);
                _userInterfaces.Add(newObject);
                return newObject;
            }
        }

        public IUserInterfaceRegister Displays { get; set; }

        public IUserInterfaceRegister Editors { get; set; }

        public void SetChildValue(ILaminarValue childValue)
        {
            if (_childValue != null)
            {
                _childValue.TypeDefinitionChanged -= ChildValue_TypeDefinitionChanged;
                _childValue.IsUserEditable.PropertyChanged -= IsUserEditable_PropertyChanged;
            }

            _childValue = childValue;
            _childValue.TypeDefinitionChanged += ChildValue_TypeDefinitionChanged;
            _childValue.IsUserEditable.PropertyChanged += IsUserEditable_PropertyChanged;
            RefreshUserInterfaces();
        }

        private void IsUserEditable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshUserInterfaces();
        }

        private void ChildValue_TypeDefinitionChanged(object sender, ITypeDefinition e)
        {
            RefreshUserInterfaces();
        }

        private void RefreshUserInterfaces()
        {
            foreach (IObservableValue<object> userInterface in _userInterfaces)
            {
                userInterface.Value = GetUIOfType(_userInterfaceType);
            }
        }

        private object GetUIOfType(string AQNOfType)
        {
            if (_childValue.TypeDefinition is not null)
            {
                if (_childValue.IsUserEditable.Value)
                {
                    if (Editors.TryGetUserInterface(AQNOfType, _childValue.TypeDefinition.EditorName, out object userInterface))
                    {
                        return userInterface;
                    }

                    if (Editors.TryGetUserInterface(AQNOfType, _instance.GetTypeInfo(_childValue.TypeDefinition.ValueType).DefaultEditor, out object defaultTypeUserInterface))
                    {
                        return defaultTypeUserInterface;
                    }
                }
                else
                {
                    if (Displays.TryGetUserInterface(AQNOfType, _childValue.TypeDefinition.DisplayName, out object userInterface))
                    {
                        return userInterface;
                    }

                    if (Displays.TryGetUserInterface(AQNOfType, _instance.GetTypeInfo(_childValue.TypeDefinition.ValueType).DefaultDisplay, out object defaultTypeUserInterface))
                    {
                        return defaultTypeUserInterface;
                    }
                }
            }

            if (Displays.TryGetUserInterface(AQNOfType, "DefaultDisplay", out object defaultDisplay))
            {
                return defaultDisplay;
            }

            return null;
        }
    }
}
