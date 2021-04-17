namespace Laminar_Core.Primitives.LaminarValue
{
    using Laminar_PluginFramework.Primitives;
    using Laminar_PluginFramework.Primitives.TypeDefinition;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Stores a well-constrained value by managing a list of <see cref="ITypeDefinition"/>
    /// </summary>
    public class LaminarValue : INotifyPropertyChanged, ILaminarValue
    {
        private object _value;
        private ILaminarValue _driver;
        private ITypeDefinition _currentTypeDefinition;
        private string _name;
        private ITypeDefinitionProvider _typeDefinitionProvider;

        public LaminarValue(ITypeDefinitionProvider provider, IObservableValue<bool> isUserEditable)
        {
            TypeDefinitionProvider = provider;
            IsUserEditable = isUserEditable;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ITypeDefinition> TypeDefinitionChanged;

        /// <summary>
        /// The <see cref="ITypeDefinition"/> which currently controls the LaminarValue
        /// </summary>
        public ITypeDefinition TypeDefinition
        {
            get => _currentTypeDefinition;
            private set
            {
                if (value != _currentTypeDefinition)
                {
                    _currentTypeDefinition = value;
                    _value = _currentTypeDefinition.DefaultValue;
                    TypeDefinitionChanged?.Invoke(this, _currentTypeDefinition);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TypeDefinition)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        /// <summary>
        /// The name of the OpenFlowValue, to be used by UI displays
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        /// <summary>
        /// The value the OpenFlowValue currently has
        /// </summary>
        public object Value
        {
            get => _driver == null ? _value : _driver.Value;
            set
            {
                _currentTypeDefinition ??= TypeDefinitionProvider.TryGetDefinitionFor(value, out ITypeDefinition typeDefinition) ? typeDefinition : null;

                if (_currentTypeDefinition.TryConstrainValue(value, out object outputVal) && (outputVal is null || !outputVal.Equals(Value)))
                {
                    _value = outputVal;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        /// <summary>
        /// Whether the OpenFlowValue should be edited by the user
        /// </summary>
        public IObservableValue<bool> IsUserEditable { get; }

        /// <summary>
        /// If not null, the <see cref="Value"/> of the Driver will determine the value of this OpenFlowValue
        /// </summary>
        public ILaminarValue Driver
        {
            get => _driver;
            set
            {
                if (_driver != null)
                {
                    _driver.PropertyChanged -= DriverPropertyChanged;
                }

                _driver = value;

                if (_driver != null)
                {
                    _driver.PropertyChanged += DriverPropertyChanged;
                    IsUserEditable.Value = false;
                }
                else
                {
                    IsUserEditable.Value = true;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public ITypeDefinitionProvider TypeDefinitionProvider
        {
            get => _typeDefinitionProvider;
            set
            {
                _typeDefinitionProvider = value;
                TypeDefinition = _typeDefinitionProvider.DefaultDefinition;
            }
        }

        /// <summary>
        /// Determines whether this OpenFlowValue can take a value. Will change <see cref="TypeDefinition"/> if required
        /// </summary>
        /// <param name="value">The value to be checked</param>
        /// <returns>True if the value can be set, false if the value cannot</returns>
        public bool CanSetValue(object value)
        {
            if (TypeDefinition != null && TypeDefinition.CanAcceptValue(value))
            {
                return true;
            }

            if (TypeDefinitionProvider.TryGetDefinitionFor(value, out ITypeDefinition typeDefinition))
            {
                TypeDefinition = typeDefinition;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clones this OpenFlowValue
        /// </summary>
        /// <returns>A new OpenFlowValue with the same properties as this one</returns>
        public ILaminarValue Clone()
        {
            ILaminarValue output = new LaminarValue(TypeDefinitionProvider, Instance.Factory.GetImplementation<IObservableValue<bool>>())
            {
                Value = Value,
                Name = Name,
            };
            output.IsUserEditable.Value = IsUserEditable.Value;
            return output;
        }

        /// <summary>
        /// An event which is called when a property changes on the <see cref="Driver"/>, and relays this change forward
        /// </summary>
        /// <param name="sender">The driver that sent the event</param>
        /// <param name="e">The name of the property</param>
        private void DriverPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
