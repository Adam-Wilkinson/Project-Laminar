using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Laminar.Domain.Notification;
using static Laminar.Domain.UnitTests.TestUtils;

namespace Laminar.Domain.UnitTests.Notification.UnitTests;

public class MappedObservableCollectionTests
{
    private readonly ObservableCollection<int> _source = [1, 2, 3, 4, 5, 6];
    private static readonly string[] SingleItemAddedList = ["10"];

    [Fact]
    public void MOC_ShouldMapCorrectly()
    {
        var sut = new MappedObservableCollection<int, string>(_source, number => number.ToString());
        
        sut.Should().Equal("1", "2", "3", "4", "5", "6");
    }

    [Fact]
    public void MOC_ShouldRaiseCorrectEvent_WhenItemAdded()
    {
        var sut = new MappedObservableCollection<int, string>(_source, number => number.ToString());

        using var mon = sut.Monitor();
        _source.Insert(3, 10);
        
        mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
            .WithSender(_source)
            .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                args.Action == NotifyCollectionChangedAction.Add &&
                SequenceEquals(args.NewItems, SingleItemAddedList) && 
                args.NewStartingIndex == 3);
    }

    [Fact]
    public void MOC_ShouldBeCorrect_WhenItemAdded()
    {
        var sut = new MappedObservableCollection<int, string>(_source, number => number.ToString());
        _source.Insert(3, 10);
        sut.Should().Equal("1", "2", "3", "10", "4", "5", "6");
    }

    [Fact]
    public void MOC_ShouldRaiseCorrectEvent_WhenItemsAdded()
    {
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());
        
        using var monitor = sut.Monitor();
        mock.CollectionChanged +=
            Raise.Event<NotifyCollectionChangedEventHandler>(mock,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<int> { 10, 11, 12 }, 3));
        
        monitor.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
            .WithSender(mock)
            .WithArgs<NotifyCollectionChangedEventArgs>(args => 
                args.Action == NotifyCollectionChangedAction.Add && 
                SequenceEquals(args.NewItems, new List<string> {"10", "11", "12"}) &&
                args.NewStartingIndex == 3);
    }

    [Fact]
    public void MOC_ShouldBeCorrect_WhenItemsAdded()
    {
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());
        
        mock.CollectionChanged +=
            Raise.Event<NotifyCollectionChangedEventHandler>(mock,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<int> { 10, 11, 12 }, 3));

        sut.Should().Equal("1", "2", "3", "10", "11", "12", "4", "5", "6");
    }
    
    [Fact]
    public void MOC_ShouldRaiseCorrectEvent_WhenItemRemoved()
    {
        var sut = new MappedObservableCollection<int, string>(_source, number => number.ToString());
        
        using var mon = sut.Monitor();
        _source.RemoveAt(3);
        
        mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
            .WithSender(_source)
            .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                args.Action == NotifyCollectionChangedAction.Remove &&
                SequenceEquals(args.OldItems, new List<string> {"4"}) && 
                args.OldStartingIndex == 3);
    }

    [Fact]
    public void MOC_ShouldBeCorrect_WhenItemRemoved()
    {
        var sut = new MappedObservableCollection<int, string>(_source, number => number.ToString());
        _source.RemoveAt(3);
        sut.Should().Equal("1", "2", "3", "5", "6");
    }

    [Fact]
    public void MOC_ShouldRaiseCorrectEvent_WhenItemsRemoved()
    {
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());
        
        using var monitor = sut.Monitor();
        
        mock.CollectionChanged +=
            Raise.Event<NotifyCollectionChangedEventHandler>(mock,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<int> { 2, 3, 4 }, 1));
        
        monitor.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
            .WithSender(mock)
            .WithArgs<NotifyCollectionChangedEventArgs>(args => 
                args.Action == NotifyCollectionChangedAction.Remove && 
                SequenceEquals(args.OldItems, new List<string> { "2", "3", "4" }) &&
                args.OldStartingIndex == 1);
    }

    [Fact]
    public void MOC_ShouldBeCorrect_WhenItemsRemoved()
    {
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());
        
        mock.CollectionChanged +=
            Raise.Event<NotifyCollectionChangedEventHandler>(mock,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<int> { 2, 3, 4 }, 1));

        sut.Should().Equal("1", "5", "6");
    }

    [Fact]
    public void MOC_ShouldRaiseCorrectEvent_WhenItemReplaced()
    {
        const int replaceIndex = 2;
        const int newItem = 10;
        int replacedItem = _source[replaceIndex];
        
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());
        using var mon = sut.Monitor();
        
        var oldItemReference = sut[replaceIndex];
        var collectionChangedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (object)newItem, (object)replacedItem, replaceIndex);

        mock.CollectionChanged += Raise.Event<NotifyCollectionChangedEventHandler>(mock, collectionChangedArgs);
        
        mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
            .WithSender(mock)
            .WithArgs<NotifyCollectionChangedEventArgs>(args => 
                args.Action == NotifyCollectionChangedAction.Replace && 
                ReferenceEquals(oldItemReference, args.OldItems![0]) &&
                SequenceEquals(args.NewItems, new List<string> { newItem.ToString() }) &&
                args.OldStartingIndex == replaceIndex);
    }

    [Fact]
    public void MOC_ShouldBeCorrect_WhenItemReplaced()
    {
        const int replaceIndex = 2;
        const int newItem = 10;
        int replacedItem = _source[replaceIndex];
        
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        mock.Count.Returns(_source.Count);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());

        var collectionChangedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, (object)replacedItem, replaceIndex);
        mock.CollectionChanged += Raise.Event<NotifyCollectionChangedEventHandler>(mock, collectionChangedArgs);
        
        sut.Should().Equal("1", "2", "10", "4", "5", "6");
    }

    [Fact]
    public void MOC_ShouldRaiseCorrectEvent_WhenItemsReplaced()
    {
        const int replaceIndex = 2;
        int[] newItems = [10, 11, 12, 13, 14];
        int[] replacedItems = [_source[replaceIndex], _source[replaceIndex + 1], _source[replaceIndex + 2], _source[replaceIndex + 3]];
        
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());
        using var mon = sut.Monitor();
        
        string[] oldItems = [sut[replaceIndex], sut[replaceIndex + 1], sut[replaceIndex + 2], sut[replaceIndex + 3]];

        var collectionChangedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, replacedItems, replaceIndex);
        mock.CollectionChanged += Raise.Event<NotifyCollectionChangedEventHandler>(mock, collectionChangedArgs);      
        
        mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
            .WithSender(mock)
            .WithArgs<NotifyCollectionChangedEventArgs>(args => 
                args.Action == NotifyCollectionChangedAction.Replace && 
                SequenceReferenceEquals(oldItems, args.OldItems) &&
                SequenceEquals(args.NewItems, newItems.Select(x => x.ToString())) &&
                args.OldStartingIndex == replaceIndex);
    }

    [Fact]
    public void MOC_ShouldBeCorrect_WhenItemsReplaced()
    {
        const int replaceIndex = 2;
        int[] newItems = [10, 11, 12, 13, 14];
        int[] replacedItems = [_source[replaceIndex], _source[replaceIndex + 1], _source[replaceIndex + 2]];
        
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());

        var collectionChangedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, replacedItems, replaceIndex);
        mock.CollectionChanged += Raise.Event<NotifyCollectionChangedEventHandler>(mock, collectionChangedArgs);
        
        sut.Should().Equal("1", "2", "10", "11", "12", "13", "14", "6");
    }

    [Fact]
    public void MOC_ShouldRaiseCorrectEvent_WhenItemMoved()
    {
        const int moveFromIndex = 2;
        const int moveToIndex = 4;
        
        var sut = new MappedObservableCollection<int, string>(_source, number => number.ToString());
        var movedItem = sut[moveFromIndex];
        using var mon = sut.Monitor();
        
        _source.Move(moveFromIndex, moveToIndex);
        mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
            .WithSender(_source)
            .WithArgs<NotifyCollectionChangedEventArgs>(args => 
                args.Action == NotifyCollectionChangedAction.Move &&
                args.OldStartingIndex == moveFromIndex &&
                args.NewStartingIndex == moveToIndex &&
                ReferenceEquals(movedItem, args.OldItems![0]));
    }

    [Fact]
    public void MOC_ShouldBeCorrect_WhenItemMoved()
    {
        const int moveFromIndex = 2;
        const int moveToIndex = 4;
        
        var sut = new MappedObservableCollection<int, string>(_source, number => number.ToString());
        var movedItem = sut[moveFromIndex];
        using var mon = sut.Monitor();
        
        _source.Move(moveFromIndex, moveToIndex);
        sut.Should().Equal("1", "2", "4", "5", "3", "6");
    }


    [Fact]
    public void MOC_ShouldRaiseCorrectEvent_WhenItemsMoved()
    {
        const int moveFromIndex = 2;
        const int moveToIndex = 3;
        
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());
        using var mon = sut.Monitor();
        
        string[] sutOldItems = [sut[moveFromIndex], sut[moveFromIndex + 1]];
        int[] sourceOldItems = [_source[moveFromIndex], _source[moveFromIndex + 1]];
        var collectionChangedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
            sourceOldItems, moveToIndex, moveFromIndex);
        mock.CollectionChanged += Raise.Event<NotifyCollectionChangedEventHandler>(mock, collectionChangedArgs);
        
        mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
            .WithSender(mock)
            .WithArgs<NotifyCollectionChangedEventArgs>(args => 
                args.Action == NotifyCollectionChangedAction.Move &&
                args.OldStartingIndex == moveFromIndex &&
                args.NewStartingIndex == moveToIndex &&
                SequenceReferenceEquals(sutOldItems, args.OldItems));
    }

    [Fact]
    public void MOC_ShouldBeCorrect_WhenItemsMoved()
    {
        const int moveFromIndex = 2;
        const int moveToIndex = 3;
        
        var mock = Substitute.For<IReadOnlyObservableCollection<int>>();
        using var sourceEnumerator = _source.GetEnumerator();
        using var mockEnumerator = mock.GetEnumerator();
        mockEnumerator.Returns(sourceEnumerator);
        var sut = new MappedObservableCollection<int, string>(mock, number => number.ToString());
        using var mon = sut.Monitor();
        
        string[] sutOldItems = [sut[moveFromIndex], sut[moveFromIndex + 1]];
        int[] sourceOldItems = [_source[moveFromIndex], _source[moveFromIndex + 1]];
        var collectionChangedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, sourceOldItems, moveToIndex, moveFromIndex);
        mock.CollectionChanged += Raise.Event<NotifyCollectionChangedEventHandler>(mock, collectionChangedArgs);

        sut.Should().Equal("1", "2", "5", "3", "4", "6");
    }

    [Fact]
    public void MOC_ShouldRaiseCorrectEvent_OnReset()
    {
        var sut = new MappedObservableCollection<int, string>(_source, number => number.ToString());
        using var mon = sut.Monitor();
        
        _source.Clear();
        
        mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
            .WithSender(_source)
            .WithArgs<NotifyCollectionChangedEventArgs>(args => 
                args.Action == NotifyCollectionChangedAction.Reset);
    }

    [Fact]
    public void MOC_ShouldBeCorrect_OnReset()
    {
        var sut = new MappedObservableCollection<int, string>(_source, number => number.ToString());
        
        _source.Clear();

        sut.Should().Equal();
    }
}