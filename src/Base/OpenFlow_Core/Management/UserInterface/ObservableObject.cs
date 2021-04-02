using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Management.UserInterface
{
    public class ObservableObject : INotifyPropertyChanged
    {
        private object _observable;

        public ObservableObject() : this(null) { }

        public ObservableObject(object initialValue)
        {
            Observable = initialValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public object Observable
        {
            get
            {
                return _observable;
            }
            set
            {
                if (value is not null && _observable != value)
                {
                    _observable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Observable)));
                }
            }
        }

        public bool HasNoListeners()
        {
            return false;
        }
    }
}
