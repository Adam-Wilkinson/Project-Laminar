using Laminar_PluginFramework.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives.LaminarValue
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
                else if (_coreDictionary.TryGetValue(key, out ILaminarValue laminarValue) && laminarValue.CanSetValue(value))
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
        public event EventHandler<ILaminarValue> ChildValueChanged;

        public void AddValue(object key, object value, bool isUserEditable)
        {
            ILaminarValue newValue = _valueFactory.Get(value, isUserEditable);
            Add(key, newValue);
        }

        public void AddValue<T>(object key, bool isUserEditable)
        {
            ILaminarValue newValue = _valueFactory.Get<T>(isUserEditable);
            Add(key, newValue);
        }

        public ILaminarValue GetValue(object key) => _coreDictionary.TryGetValue(key, out ILaminarValue value) ? value : null;

        public void SetValueName(string name)
        {
            _storeName = name;
            foreach (ILaminarValue value in _coreDictionary.Values)
            {
                value.Name = _storeName;
            }
        }

        public void CopyFrom(ILaminarValueStore copyFrom)
        {
            foreach (KeyValuePair<object, ILaminarValue> kvp in copyFrom)
            {
                Add(kvp.Key, (ILaminarValue)kvp.Value.Clone());
            }
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
                ChildValueChanged?.Invoke(this, (ILaminarValue)sender);
            }
        }

        private void Add(object key, ILaminarValue value)
        {
            value.Name = _storeName;
            _coreDictionary[key] = value;
            value.PropertyChanged += AnyValue_PropertyChanged;
            ChangedAtKey?.Invoke(this, key);
        }

        public IEnumerator<KeyValuePair<object, ILaminarValue>> GetEnumerator() => _coreDictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _coreDictionary.GetEnumerator();
    }
}
