using System.Collections;
using System.Collections.Specialized;

namespace Laminar.Domain.Extensions;

public static class NotifyCollectionChangedEventArgsExtensions
{
    extension(NotifyCollectionChangedEventArgs)
    {
        public static NotifyCollectionChangedEventArgs InsertRange(IList newItems, int index) 
            => new(NotifyCollectionChangedAction.Add, newItems, index);
        
        public static NotifyCollectionChangedEventArgs Insert(object newItem, int index)
            => new(NotifyCollectionChangedAction.Add, newItem, index);
        
        public static NotifyCollectionChangedEventArgs RemoveRange(IList oldItems, int index)
            => new(NotifyCollectionChangedAction.Remove, oldItems, index);
        
        public static NotifyCollectionChangedEventArgs Remove(object oldItem, int index)
            => new(NotifyCollectionChangedAction.Remove, oldItem, index);
        
        public static NotifyCollectionChangedEventArgs ReplaceRange(IList newItems, IList oldItems, int index)
            => new(NotifyCollectionChangedAction.Replace, newItems, oldItems, index);
        
        public static NotifyCollectionChangedEventArgs Replace(object newItem, object oldItem, int index)
            => new(NotifyCollectionChangedAction.Replace, newItem, oldItem, index);
        
        public static NotifyCollectionChangedEventArgs Move(object movedObject, int newIndex, int oldIndex)
            => new(NotifyCollectionChangedAction.Move, movedObject, newIndex, oldIndex);
        
        public static NotifyCollectionChangedEventArgs MoveRange(IList movedObject, int newIndex, int oldIndex)
            => new(NotifyCollectionChangedAction.Move, movedObject, newIndex, oldIndex);
    }
}