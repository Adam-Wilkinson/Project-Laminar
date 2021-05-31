using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives.ObservableCollectionMapper
{
    public interface IObservableCollectionMapper<TIn, TOut>
    {
        ReadOnlyObservableCollection<TOut> Map(INotifyCollectionChanged inputCollection);
    }
}
