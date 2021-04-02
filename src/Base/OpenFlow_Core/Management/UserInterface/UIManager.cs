using OpenFlow_Core.Management;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Management.UserInterface
{
    public class UIManager
    {
        private ILaminarValue _childValue;
        private string _userInterfaceType;
        private readonly List<ObservableObject> _userInterfaces = new();

        public object this[string key]
        {
            get
            {
                CleanUserInterfaces();
                if (_userInterfaceType == null)
                {
                    _userInterfaceType = key;
                }

                if (_userInterfaceType != key)
                {
                    throw new Exception($"{nameof(UIManager)} cannot handle multiple types of interface at once!");
                }

                ObservableObject newObject = new(GetUIOfType(key));
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
            foreach (ObservableObject userInterface in _userInterfaces)
            {
                userInterface.Observable = newUserInterface;
            }
        }

        private void CleanUserInterfaces()
        {
            int i = 0;
            while (i < _userInterfaces.Count)
            {
                if (_userInterfaces[i].HasNoListeners())
                {
                    Debug.WriteLine("Cleaning is working");
                    _userInterfaces.RemoveAt(i);
                }
                else
                {
                    i++;
                }
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

    }
}
