using FluentAssertions;
using Laminar.Domain.Notification;

namespace Laminar.Domain.UnitTests.Notification.UnitTests;

public class FlattenedObservableTreeTests
{
    private FlattenedObservableTree<int> _sut;

    [Fact]
    public void FOT_ShouldFlatten_ComplexTree()
    {
        List<int> rootListFirstElement = new() { 1, 2 };

        int rootListSecondElement = 3;

        List<int> firstNestFirstElement = new() { 4, 5 };
        int firstNestSecondElement = 6;
        List<int> firstNestThirdElement = new() { 7, 8, 9 };
        List<object> rootListThirdElement = new() { firstNestFirstElement, firstNestSecondElement, firstNestThirdElement };

        List<int> rootListFourthElement = new() { 10, 11 };

        List<object> rootList = new() { rootListFirstElement, rootListSecondElement, rootListThirdElement, rootListFourthElement };
        _sut = new(rootList);


        _sut.Should().BeEquivalentTo(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
    }
}
