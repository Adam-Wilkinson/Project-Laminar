using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Primitives.LaminarValue
{
    public class LaminarValueStore : ILaminarValueStore
    {
        private readonly ILaminarValueFactory _valueFactory;
        private readonly Dictionary<object, ILaminarValue> _coreDictionary = new();
        private string _storeName;

        public LaminarValueStore(ILaminarValueFactory valueFactory)
        {
            _valueFactory = valueFactory;
        }

        public object this[object key] 
        {
            get => _coreDictionary[key].Value;
            set
            {
                if (value is null && _coreDictionary.ContainsKey(key))
                {
                    RemoveValue(key);
                }
                if (_coreDictionary.TryGetValue(key, out ILaminarValue laminarValue) && laminarValue.CanSetValue(value))
                {
                    laminarValue.Value = value;
                }
                else
                {
                    AddValue(key, value, false);
                }
            }
        }

        public int Count => _coreDictionary.Count;

        public string StoreName
        {
            get => _storeName;
            set
            {
                _storeName = value;
                SetValueName(_storeName);
            }
        }

        public event EventHandler<object> ChangedAtKey;
        public event EventHandler AnyValueChanged;

        public void AddValue(object key, object value, bool isUserEditable)
        {
            ILaminarValue newValue = _valueFactory.Get(value, isUserEditable);
            newValue.Name = _storeName;
            _coreDictionary[key] = newValue;
            newValue.PropertyChanged += AnyValue_PropertyChanged;
            ChangedAtKey?.Invoke(this, key);
        }

        public ILaminarValue GetValue(object key) => _coreDictionary.TryGetValue(key, out ILaminarValue value) ? value : null;

        public void SetValueName(string name)
        {
            foreach (ILaminarValue value in _coreDictionary.Values)
            {
                value.Name = name;
            }
        }

        public ILaminarValueStore Clone()
        {
            LaminarValueStore newStore = new(_valueFactory);
            foreach (KeyValuePair<object, ILaminarValue> kvp in _coreDictionary)
            {
                newStore.AddValue(kvp.Key, kvp.Value.TypeDefinitionProvider, kvp.Value.IsUserEditable);
            }
            return newStore;
        }


        private void RemoveValue(object key)
        {
            _coreDictionary[key].PropertyChanged -= AnyValue_PropertyChanged;
            _coreDictionary.Remove(key);
            ChangedAtKey?.Invoke(this, key);
        }

        private void AnyValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ILaminarValue.Value))
            {
                AnyValueChanged?.Invoke(this, new EventArgs());
            }
        }

    }
}
