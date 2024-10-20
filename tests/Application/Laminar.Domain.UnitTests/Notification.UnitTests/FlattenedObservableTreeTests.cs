using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FluentAssertions;
using Laminar.Domain.Notification;

namespace Laminar.Domain.UnitTests.Notification.UnitTests;

public class FlattenedObservableTreeTests
{
    readonly ObservableCollection<object> _tree = new()
    {
        new ObservableCollection<int>() { 1, 2, },
        3,
        new ObservableCollection<object>()
        {
            new ObservableCollection<int>() { 4, 5 },
            6,
            new ObservableCollection<int>() { 7, 8, 9 },
        },
        new ObservableCollection<int>() {10, 11 },
    };

    private FlattenedObservableTree<int>? _sut;

    [Fact]
    public void FOT_ShouldFlattenCorrectly()
    {
        _sut = new(_tree);

        _sut.Should().Equal(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
    }

    [Fact]
    public void FOT_ShouldRaiseCorrectEvent_WhenItemAdded()
    {
        _sut = new(_tree);
        using var mon = _sut.Monitor();
        ((ObservableCollection<int>)((ObservableCollection<object>)_tree[2])[2]).Insert(1, 13);

        mon.Should().Raise(nameof(FlattenedObservableTree<int>.CollectionChanged))
            .WithSender(_sut)
            .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                args.Action == NotifyCollectionChangedAction.Add &&
                args.NewItems!.Count == 1 &&
                (int)args.NewItems[0]! == 13 &&
                args.NewStartingIndex == 7);
    }

    [Fact]
    public void FOT_ShouldFlattenCorrectly_WhenItemAdded()
    {
        _sut = new(_tree);
        ((ObservableCollection<int>)((ObservableCollection<object>)_tree[2])[2]).Insert(1, 13);

        _sut.Should().Equal(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 13, 8, 9, 10, 11 });
    }

    [Fact]
    public void FOT_ShouldRaiseCorrectEvent_WhenSubtreeAdded()
    {
        _sut = new(_tree);
        using var mon = _sut.Monitor();
        ((ObservableCollection<object>)_tree[2]).Insert(2, new ObservableCollection<object>() { 13, new ObservableCollection<int>() { 14, 15 } });

        mon.Should().Raise(nameof(FlattenedObservableTree<int>.CollectionChanged))
            .WithSender(_sut)
            .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                args.Action == NotifyCollectionChangedAction.Add &&
                TypelessSequenceEqual(args.NewItems, new List<int>() { 13, 14, 15 }) &&
                args.NewStartingIndex == 6
            );
    }

    [Fact]
    public void FOT_ShouldFlattenCorrectly_WhenSubtreeAdded()
    {
        _sut = new(_tree);
        ((ObservableCollection<object>)_tree[2]).Insert(2, new ObservableCollection<object>() { 13, new ObservableCollection<int>() { 14, 15 } });

        _sut.Should().Equal(new List<int>() { 1, 2, 3, 4, 5, 6, 13, 14, 15, 7, 8, 9, 10, 11 });
    }

    [Fact]
    public void FOT_ShouldRaiseCorrectEvent_WhenItemRemoved()
    {
        _sut = new(_tree);
        ((ObservableCollection<int>)((ObservableCollection<object>)_tree[2])[2]).RemoveAt(1);

        _sut.Should().Equal(new List<int> { 1, 2, 3, 4, 5, 6, 7, 9, 10, 11 });
    }

    [Fact]
    public void FOT_ShouldFlattenCorrectly_WhenItemRemoved()
    {
        _sut = new(_tree);
        using var mon = _sut.Monitor();
        ((ObservableCollection<int>)((ObservableCollection<object>)_tree[2])[2]).RemoveAt(1);

        mon.Should().Raise(nameof(FlattenedObservableTree<int>.CollectionChanged))
            .WithSender(_sut)
            .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                args.Action == NotifyCollectionChangedAction.Remove &&
                args.OldStartingIndex == 7 &&
                TypelessSequenceEqual(args.OldItems, new List<int>() { 8 })
            );
    }

    [Fact]
    public void FOT_ShouldRaiseCorrectEvent_WhenSubtreeRemoved()
    {
        _sut = new(_tree);
        using var mon = _sut.Monitor();

        _tree.RemoveAt(2);
        mon.Should().Raise(nameof(FlattenedObservableTree<int>.CollectionChanged))
            .WithSender(_sut)
            .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                args.Action == NotifyCollectionChangedAction.Remove &&
                args.OldStartingIndex == 3 &&
                TypelessSequenceEqual(args.OldItems, new List<int>() { 4, 5, 6, 7, 8, 9 }));
    }

    [Fact]
    public void FOT_ShouldFlattenCorrectly_WhenSubtreeRemoved()
    {
        _sut = new(_tree);
        _tree.RemoveAt(2);

        _sut.Should().Equal(new List<int> { 1, 2, 3, 10, 11 });
    }

    [Fact]
    public void FOT_ShouldNotChange_WhenItemAddedRemoved()
    {
        _sut = new(_tree);

        using (var mon = _sut.Monitor())
        {
            ObservableCollection<int> newItems = new() { 12, 13, 14 };
            _tree.Add(newItems);

            mon.Should().Raise(nameof(FlattenedObservableTree<int>.CollectionChanged))
                .WithSender(_sut)
                .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                    args.Action == NotifyCollectionChangedAction.Add &&
                    args.NewStartingIndex == 11 &&
                    TypelessSequenceEqual(args.NewItems, new List<int>() { 12, 13, 14 })
                );
        }

        using (var mon2 = _sut.Monitor())
        {
            _tree.RemoveAt(4);

            mon2.Should().Raise(nameof(FlattenedObservableTree<int>.CollectionChanged))
                .WithSender(_sut)
                .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                    args.Action == NotifyCollectionChangedAction.Remove &&
                    args.OldStartingIndex == 11 &&
                    TypelessSequenceEqual(args.OldItems, new List<int>() { 12, 13, 14 })
                );
        }

        _sut.Should().Equal(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
    }

    [Fact]
    public void FOT_ShouldRaiseCorrectEvent_AfterReset()
    {
        _sut = new(_tree);
        using var mon = _sut.Monitor();
        _tree.Clear();

        mon.Should().Raise(nameof(FlattenedObservableTree<int>.CollectionChanged))
            .WithSender(_sut)
            .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                args.Action == NotifyCollectionChangedAction.Reset
            );

    }

    [Fact]
    public void FOT_ShouldBeEmpty_AfterReset()
    {
        _sut = new(_tree);
        _tree.Clear();

        _sut.Should().BeEmpty();
    }

    [Fact]
    public void FOT_ShouldRaiseCorrectEvent_WhenSubtreeMoved()
    {
        _sut = new(_tree);
        using var mon = _sut.Monitor();

        _tree.Move(0, 2);

        mon.Should().Raise(nameof(FlattenedObservableTree<int>.CollectionChanged))
            .WithSender(_sut)
            .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                args.Action == NotifyCollectionChangedAction.Move &&
                args.OldStartingIndex == 0 &&
                args.NewStartingIndex == 7 &&
                TypelessSequenceEqual(args.NewItems, new List<int>() { 1, 2 }));
    }

    [Fact]
    public void FOT_ShouldFlattenedCorrectly_WhenSubtreeMoved()
    {
        _sut = new(_tree);
        _tree.Move(0, 2);

        _sut.Should().Equal(new List<int> { 3, 4, 5, 6, 7, 8, 9, 1, 2, 10, 11 });
    }

    [Fact]
    public void FOT_ShouldRaiseCorrectEvent_WhenItemMoved()
    {
        _sut = new(_tree);
        using var mon = _sut.Monitor();

        _tree.Move(1, 2);

        mon.Should().Raise(nameof(FlattenedObservableTree<int>.CollectionChanged))
            .WithSender(_sut)
            .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                args.Action == NotifyCollectionChangedAction.Move &&
                args.OldStartingIndex == 2 &&
                args.NewStartingIndex == 8 &&
                TypelessSequenceEqual(args.NewItems, new List<int>() { 3 }));
    }

    [Fact]
    public void FOT_ShouldFlattenCorrectly_WhenItemMoved()
    {
        _sut = new(_tree);
        _tree.Move(1, 2);

        _sut.Should().Equal(new List<int>() { 1, 2, 4, 5, 6, 7, 8, 9, 3, 10, 11 });
    }

    private static bool TypelessSequenceEqual(IEnumerable? firstList, IEnumerable? secondList)
    {
        if (firstList is null)
        {
            return secondList is null;
        }

        if (secondList is null)
        {
            return false;
        }

        IEnumerator first = firstList.GetEnumerator();
        IEnumerator second = secondList.GetEnumerator();

        while (first.MoveNext())
        {
            if (!second.MoveNext())
            {
                return false;
            }

            if (!(first.Current.Equals(second.Current)))
            {
                return false;
            }
        }

        return !first.MoveNext();
    }
}
