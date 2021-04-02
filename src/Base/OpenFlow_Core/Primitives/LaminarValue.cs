namespace OpenFlow_Core.Primitives
{
    using OpenFlow_PluginFramework.Primitives;
    using OpenFlow_PluginFramework.Primitives.TypeDefinition;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Stores a well-constrained value by managing a list of <see cref="ITypeDefinition"/>
    /// </summary>
    public class LaminarValue : INotifyPropertyChanged, ILaminarValue
    {
        private object _value;
        private bool _isEditable;
        private ILaminarValue _driver;
        private ITypeDefinition _currentTypeDefinition;
        private string _name;
        private ITypeDefinitionProvider _typeDefinitionProvider;

        public LaminarValue()
        {
            // _typeDefinitionProvider = new AutoTypeDefinitionProvider();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The <see cref="ITypeDefinition"/> which currently governs the OpenFlowValue
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUserEditable)));
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
                _currentTypeDefinition ??= TypeDefinitionManager.TryGetDefinitionFor(value, out ITypeDefinition typeDefinition) ? typeDefinition : null;

                if (_currentTypeDefinition != null && _currentTypeDefinition.TryConstrainValue(value, out object outputVal) && !outputVal.Equals(Value))
                {
                    _value = outputVal;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        /// <summary>
        /// Whether the OpenFlowValue should be edited by the user
        /// </summary>
        public bool IsUserEditable
        {
            get => _isEditable;
            set
            {
                if (_isEditable != value)
                {
                    _isEditable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUserEditable)));
                }
            }
        }

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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                    IsUserEditable = false;
                }
                else
                {
                    IsUserEditable = true;
                }
            }
        }

        public ITypeDefinitionProvider TypeDefinitionManager
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

            if (TypeDefinitionManager.TryGetDefinitionFor(value, out ITypeDefinition typeDefinition))
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
        public ILaminarValue Clone() => new LaminarValue()
        {
            TypeDefinitionManager = TypeDefinitionManager,
            Value = Value,
            IsUserEditable = IsUserEditable,
            Name = Name,
        };

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
