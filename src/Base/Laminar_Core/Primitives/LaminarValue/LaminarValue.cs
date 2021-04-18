namespace Laminar_Core.Primitives.LaminarValue
{
    using Laminar_PluginFramework.Primitives;
    using Laminar_PluginFramework.Primitives.TypeDefinition;
    using System;

    public class LaminarValue : DependentValue<object>, ILaminarValue
    {
        private readonly ITypeDefinitionProvider _typeDefinitionProvider;
        private ITypeDefinition _currentTypeDefinition;
        private string _name;

        public LaminarValue(ITypeDefinitionProvider provider, IObservableValue<bool> isUserEditable, IObservableValue<bool> hasDependency) : base(hasDependency)
        {
            _typeDefinitionProvider = provider;
            TypeDefinition = _typeDefinitionProvider.DefaultDefinition;
            IsUserEditable = isUserEditable;
            hasDependency.OnChange += (b) =>
            {
                IsUserEditable.Value = !b;
            };
            OnChange += (val) => NotifyPropertyChanged(nameof(TrueValue));
        }

        public event EventHandler<ITypeDefinition> TypeDefinitionChanged;

        public IObservableValue<bool> IsUserEditable { get; }

        public ITypeDefinition TypeDefinition
        {
            get => _currentTypeDefinition;
            private set
            {
                if (value != _currentTypeDefinition)
                {
                    _currentTypeDefinition = value;
                    TypeDefinitionChanged?.Invoke(this, _currentTypeDefinition);
                    Value = _currentTypeDefinition.DefaultValue;
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        public object TrueValue { get => Value; set => Value = value; }

        public override object Value
        {
            set
            {
                _currentTypeDefinition ??= _typeDefinitionProvider.TryGetDefinitionFor(value, out ITypeDefinition typeDefinition) ? typeDefinition : null;

                if (_currentTypeDefinition.TryConstrainValue(value, out object outputVal))
                {
                    base.Value = outputVal;
                }
            }
        }

        public bool CanSetValue(object value)
        {
            if (TypeDefinition != null && TypeDefinition.CanAcceptValue(value))
            {
                return true;
            }

            if (_typeDefinitionProvider.TryGetDefinitionFor(value, out ITypeDefinition typeDefinition))
            {
                TypeDefinition = typeDefinition;
                return true;
            }

            return false;
        }

        public override ILaminarValue Clone()
        {
            ILaminarValue output = new LaminarValue(_typeDefinitionProvider, Instance.Factory.GetImplementation<IObservableValue<bool>>(), Instance.Factory.GetImplementation<IObservableValue<bool>>())
            {
                Value = Value,
                Name = Name,
            };
            output.IsUserEditable.Value = IsUserEditable.Value;
            return output;
        }
    }
}
