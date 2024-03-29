﻿namespace Laminar_Core.Primitives.LaminarValue
{
    using Laminar_PluginFramework.Primitives;
    using Laminar_PluginFramework.Primitives.TypeDefinition;
    using System;

    public class LaminarValue : DependentValue<object>, ILaminarValue
    {
        private readonly IObjectFactory _factory;
        private ITypeDefinition _currentTypeDefinition;
        private ITypeDefinitionProvider _typeDefinitionProvider;
        private string _name;

        public LaminarValue(ITypeDefinitionProvider provider, IObservableValue<bool> isUserEditable, IObjectFactory factory) : base()
        {
            SetTypeDefinitionProvider(provider);
            IsUserEditable = isUserEditable;
            _factory = factory;
            OnChange += (o, val) => NotifyPropertyChanged(nameof(TrueValue));
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

                if (_currentTypeDefinition is not null && _currentTypeDefinition.TryConstrainValue(value, out object outputVal))
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
            ILaminarValue output = new LaminarValue(_typeDefinitionProvider, IsUserEditable.Clone(), _factory);
            CloneTo(output);
            return output;
        }

        public override void SetDependency<TDep>(IObservableValue<TDep> dep, Func<TDep, object> conversion)
        {
            base.SetDependency(dep, conversion);
            IsUserEditable.Value = false;
        }

        public override void RemoveDependency<TDep>()
        {
            base.RemoveDependency<TDep>();
            IsUserEditable.Value = true;
        }

        public void SetTypeDefinitionProvider(ITypeDefinitionProvider provider)
        {
            if (provider == _typeDefinitionProvider)
            {
                return;
            }

            _typeDefinitionProvider = provider;
            TypeDefinition = _typeDefinitionProvider.DefaultDefinition;
        }

        public void CloneTo(ILaminarValue value)
        {
            value.SetTypeDefinitionProvider(_typeDefinitionProvider);
            value.Value = Value;
            value.Name = Name;
            value.IsUserEditable.Value = IsUserEditable.Value;
        }
    }
}
