using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Laminar.Domain.Extensions;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Collections;
using static Laminar.Domain.UnitTests.TestUtils;

namespace Laminar.Domain.UnitTests.Notification.UnitTests;

public class SourcedObservableCollectionTests
{
    private readonly ObservableCollection<int> _source = [1, 2, 3, 4, 5, 6];
    
    [Fact]
    public void ShouldInitializeCorrectly()
    {
        SourcedObservableCollection<int> sut = new(_source);
        sut.Should().Equal(1, 2, 3, 4, 5, 6);
    }
    
    public class Add
    {
        private readonly ObservableCollection<int> _source = [1, 2, 3, 4, 5, 6];
        
        [Fact]
        public void ShouldUpdateList()
        {
            SourcedObservableCollection<int> sut = new(_source) { 7 };

            sut.Should().Equal(1, 2, 3, 4, 5, 6, 7);
        }

        [Fact]
        public void ShouldRaiseEvent()
        {
            SourcedObservableCollection<int> sut = new(_source);

            using var mon = sut.Monitor();
            
            sut.Add(7);
            
            mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
                .WithSender(sut)
                .WithArgs<NotifyCollectionChangedEventArgs>(args => 
                    args.Action == NotifyCollectionChangedAction.Add &&
                    SequenceEquals(args.NewItems, new[] { 7 }) &&
                    args.NewStartingIndex == 6);
        }
    }
    
    public class Remove
    {
        private readonly ObservableCollection<int> _source = [1, 2, 3, 4, 5, 6];

        [Fact]
        public void ShouldUpdateList()
        {
            SourcedObservableCollection<int> sut = new(_source);
            
            sut.Remove(4);
            
            sut.Should().Equal(1, 2, 3, 5, 6);
        }

        [Fact]
        public void ShouldRaiseEvent()
        {
            SourcedObservableCollection<int> sut = new(_source);
            
            using var mon = sut.Monitor();
            
            sut.Remove(4);
            
            mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
                .WithSender(sut)
                .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                    args.Action == NotifyCollectionChangedAction.Remove &&
                    SequenceEquals(args.OldItems, 4.Yield()) &&
                    args.OldStartingIndex == 3);
        }
    }
    
    public class RemoveRange
    {
        private readonly ObservableCollection<int> _source = [1, 2, 3, 4, 5, 6];

        [Fact]
        public void ShouldUpdateList()
        {
            SourcedObservableCollection<int> sut = new(_source);
            
            sut.RemoveRange(2, 3);
            
            sut.Should().Equal(1, 2, 6);
        }

        [Fact]
        public void ShouldRaiseEvent()
        {
            SourcedObservableCollection<int> sut = new(_source);
            using var mon = sut.Monitor();
            
            sut.RemoveRange(2, 3);
            
            mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
                .WithSender(sut)
                .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                    args.Action == NotifyCollectionChangedAction.Remove &&
                    SequenceEquals(args.OldItems, new[] { 3, 4, 5 }) &&
                    args.OldStartingIndex == 2);
        }
    }
    
    public class InsertRange
    {
        private readonly ObservableCollection<int> _source = [1, 2, 3, 4, 5, 6];

        [Fact]
        public void ShouldUpdateList()
        {
            SourcedObservableCollection<int> sut = new(_source);
            
            sut.InsertRange(4, [10, 11, 12]);

            sut.Should().Equal(1, 2, 3, 4, 10, 11, 12, 5, 6);
        }

        [Fact]
        public void ShouldRaiseEvent()
        {
            SourcedObservableCollection<int> sut = new(_source);
            using var mon = sut.Monitor();
            
            sut.InsertRange(4, [10, 11, 12]);
            
            mon.Should().Raise(nameof(INotifyCollectionChanged.CollectionChanged))
                .WithSender(sut)
                .WithArgs<NotifyCollectionChangedEventArgs>(args =>
                    args.Action == NotifyCollectionChangedAction.Add &&
                    SequenceEquals(args.NewItems, new[] { 10, 11, 12 }) &&
                    args.NewStartingIndex == 4);
        }
    }
    
    public class SourceUpdated
    {
        private readonly ObservableCollection<int> _source = [1, 2, 3, 4, 5, 6];

        [Fact]
        public void ShouldUpdateOnAdd()
        {
            SourcedObservableCollection<int> sut = new(_source);
        }
    }
    
    public class SyncFromSource
    {
        private readonly List<int> _source = [1, 2, 3, 4, 5, 6];

        public static TheoryData<int[]> SourceUpdateTestCases =>
        [
            // Removal of an item
            [1, 2, 4, 5, 6],
            
            // Addition  of an item
            [1, 2, 3, 4, 5, 6, 10],
            
            // Removal and addition of several items, including to and from the endpoints
            [1, 2, 4, 5, 6, 10, 11, 12],
            [2, 4, 6, 8, 10],
            [10, 1, 2, 3, 6 ],
            [1, 2, 11, 3, 4, 5 ],
            [2, 3, 4],
            
            // Testing movement
            [1, 2, 4, 3, 5, 6 ],
            [6, 5, 4, 3, 2, 1 ],
            [10, 5, 3, 8, 1, 4, 11 ],
            [1, 4, 100, 3, 7, 6, 10, 5],
            [2, 3, 10, 6, 4, 5],
        ];

        [Theory, MemberData(nameof(SourceUpdateTestCases))]
        public void ShouldGiveSetEquality(int[] testCase)
        {
            SourcedObservableCollection<int> sut = new(_source);
            
            _source.Clear();
            _source.AddRange(testCase);
            sut.SyncFromSource();
            
            Assert.True(SetEquals(sut, _source));
        }

        [Theory, MemberData(nameof(SourceUpdateTestCases))]
        public void ShouldGiveSequenceEquality(int[] testCase)
        {
            SourcedObservableCollection<int> sut = new(_source);
            
            _source.Clear();
            _source.AddRange(testCase);
            sut.SyncFromSource();
            
            Assert.True(SequenceEquals(sut, _source));
        }
        
        [Fact]
        public void ShouldRespondToItemAdded()
        {
            SourcedObservableCollection<int> sut = new(_source);
            
            _source.Insert(3, 10);
            sut.SyncFromSource();
            
            Assert.True(SequenceEquals(sut, _source));
        }

        [Fact]
        public void ShouldRespondToItemRemoved()
        {
            SourcedObservableCollection<int> sut = new(_source);
            
            _source.RemoveAt(3);
            sut.SyncFromSource();
            
            Assert.True(SequenceEquals(sut, _source));
        }

        [Fact]
        public void ShouldHandleAllPermutations()
        {
            const int permutationCount = 2 * 3 * 4;
            
            for (int i = 0; i < permutationCount; i++)
            {
                int[] source = [0, 1, 2, 3];
                SourcedObservableCollection<int> sut = new(source);
                
                source[0] = (permutationCount / 4) % 4;
                source[1] = (permutationCount / 3) % 4;
                source[2] = (permutationCount / 2) % 4;
                source[3] = (permutationCount / 1) % 4;

                if (source[0] == source[1] || source[0] == source[2] || source[0] == source[3] ||
                    source[1] == source[2] || source[1] == source[3] || source[2] == source[3])
                {
                    continue;
                }
                
                sut.SyncFromSource();
                
                Assert.True(SequenceEquals(sut, _source));
            }
        }
    }
}