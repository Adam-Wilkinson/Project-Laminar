using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives.UserInterface
{
    public class UserInterfaceManager : IUserInterfaceManager
    {
        private ILaminarValue _childValue;
        private string _userInterfaceType;
        private readonly List<IObservableValue<object>> _userInterfaces = new();

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

                IObservableValue<object> newObject = Instance.Factory.GetImplementation<IObservableValue<object>>();
                newObject.Value = GetUIOfType(key);
                _userInterfaces.Add(newObject);
                return newObject;
            }
        }

        public void SetChildValue(ILaminarValue childValue)
        {
            if (_childValue != null)
            {
                _childValue.PropertyChanged -= ChildValue_PropertyChanged;
            }

            _childValue = childValue;
            _childValue.PropertyChanged += ChildValue_PropertyChanged;
            RefreshUserInterfaces();
        }

        private void ChildValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ILaminarValue.IsUserEditable) or nameof(ILaminarValue.TypeDefinition))
            {
                RefreshUserInterfaces();
            }
        }

        private void RefreshUserInterfaces()
        {
            object newUserInterface = GetUIOfType(_userInterfaceType);
            foreach (IObservableValue<object> userInterface in _userInterfaces)
            {
                userInterface.Value = newUserInterface;
            }
        }

        private object GetUIOfType(string AQNOfType)
        {
            if (_childValue.TypeDefinition is not null)
            {
                if (_childValue.IsUserEditable)
                {
                    if (Instance.Current.RegisteredEditors.TryGetUserInterface(AQNOfType, _childValue.TypeDefinition.EditorName, out object userInterface))
                    {
                        return userInterface;
                    }

                    if (Instance.Current.RegisteredEditors.TryGetUserInterface(AQNOfType, Instance.Current.GetTypeInfo(_childValue.TypeDefinition.ValueType).DefaultEditor, out object defaultTypeUserInterface))
                    {
                        return defaultTypeUserInterface;
                    }
                }
                else
                {
                    if (Instance.Current.RegisteredDisplays.TryGetUserInterface(AQNOfType, _childValue.TypeDefinition.DisplayName, out object userInterface))
                    {
                        return userInterface;
                    }

                    if (Instance.Current.RegisteredDisplays.TryGetUserInterface(AQNOfType, Instance.Current.GetTypeInfo(_childValue.TypeDefinition.ValueType).DefaultDisplay, out object defaultTypeUserInterface))
                    {
                        return defaultTypeUserInterface;
                    }
                }
            }

            if (Instance.Current.RegisteredDisplays.TryGetUserInterface(AQNOfType, "DefaultDisplay", out object defaultDisplay))
            {
                return defaultDisplay;
            }

            return null;
        }

        public IUserInterfaceManager Clone()
        {
            return new UserInterfaceManager() { _childValue = _childValue };
        }
    }
}
