using Laminar.Implementation.Storage.FileExplorer.Graph;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Graph.UnitTests;

public class FileSystemItemRepositoryTests
{
    private static readonly FileSystemGraph.MutationToken Token = FileSystemGraph.GetTestingToken();
    
    public class TryGetItem
    {
        [Fact]
        public void ShouldReturnFalseWhenItemDoesNotExist()
        {
            var sut = CreateRepository();

            var found = sut.TryGetItem("testPath", out var item);

            found.Should().BeFalse();
            item.Should().BeNull();
        }

        [Fact]
        public void ShouldReturnAddedItem()
        {
            var item = MockFactory.CreateItem();
            var sut = CreateRepository();

            sut.Add(Token, item);

            var found = sut.TryGetItem(item.Path, out var result);

            found.Should().BeTrue();
            result.Should().BeSameAs(item);
        }

        [Fact]
        public void ShouldReturnChildItemWhenFolderIsAdded()
        {
            var child = MockFactory.CreateItem();
            var folder = MockFactory.CreateFolder(contents: [child]);
            var sut = CreateRepository();

            sut.Add(Token, folder);

            var found = sut.TryGetItem(child.Path, out var result);

            found.Should().BeTrue();
            result.Should().BeSameAs(child);
        }
    }

    public class Add
    {
        [Fact]
        public void ShouldAddItem()
        {
            var item = MockFactory.CreateItem();
            var sut = CreateRepository();

            sut.Add(Token, item);

            sut.TryGetItem(item.Path, out var result).Should().BeTrue();
            result.Should().BeSameAs(item);
        }

        [Fact]
        public void ShouldRecursivelyAddFolderContents()
        {
            var grandchild = MockFactory.CreateItem();
            var childFolder = MockFactory.CreateFolder(contents: [grandchild]);
            var folder = MockFactory.CreateFolder(contents: [childFolder]);
            var sut = CreateRepository();

            sut.Add(Token, folder);

            sut.TryGetItem(grandchild.Path, out var result).Should().BeTrue();
            result.Should().BeSameAs(grandchild);
        }
    }

    public class Remove
    {
        [Fact]
        public void ShouldRemoveItem()
        {
            var item = MockFactory.CreateItem();
            var sut = CreateRepository();
            sut.Add(Token, item);

            sut.Remove(Token, item);

            sut.TryGetItem(item.Path, out _).Should().BeFalse();
        }

        [Fact]
        public void ShouldRecursivelyRemoveFolderContents()
        {
            var grandchild = MockFactory.CreateItem();
            var childFolder = MockFactory.CreateFolder(contents: [grandchild]);
            var folder = MockFactory.CreateFolder(contents: [childFolder]);
            var sut = CreateRepository();
            sut.Add(Token, folder);

            sut.Remove(Token, folder);

            sut.TryGetItem(folder.Path, out _).Should().BeFalse();
            sut.TryGetItem(childFolder.Path, out _).Should().BeFalse();
            sut.TryGetItem(grandchild.Path, out _).Should().BeFalse();
        }

        [Fact]
        public void ShouldIgnoreRemovingItemThatDoesNotExist()
        {
            var item = MockFactory.CreateItem();
            var sut = CreateRepository();

            Action act = () => sut.Remove(Token, item);

            act.Should().NotThrow();
        }
    }

    public class DetachOutdatedItems
    {
        [Fact]
        public void ShouldContainRemovedItem()
        {
            var item = MockFactory.CreateItem();
            var sut = CreateRepository();
            sut.Add(Token, item);
            sut.Remove(Token, item);

            var outdated = sut.DetachOutdatedItems();

            outdated.TryGetItem(item.Path, out var result).Should().BeTrue();
            result.Should().BeSameAs(item);
        }

        [Fact]
        public void ShouldContainRemovedChildren()
        {
            var child = MockFactory.CreateItem();
            var folder = MockFactory.CreateFolder(contents: [child]);
            var sut = CreateRepository();
            sut.Add(Token, folder);

            sut.Remove(Token, folder);
            var outdated = sut.DetachOutdatedItems();

            outdated.TryGetItem(folder.Path, out var removedFolder).Should().BeTrue();
            removedFolder.Should().BeSameAs(folder);

            outdated.TryGetItem(child.Path, out var removedChild).Should().BeTrue();
            removedChild.Should().BeSameAs(child);
        }

        [Fact]
        public void ShouldClearOutdatedItemsAfterDetach()
        {
            var item = MockFactory.CreateItem();
            var sut = CreateRepository();
            sut.Add(Token, item);
            sut.Remove(Token, item);

            sut.DetachOutdatedItems();
            var outdated = sut.DetachOutdatedItems();

            outdated.TryGetItem(item.Path, out _).Should().BeFalse();
        }

        [Fact]
        public void ShouldDiscardOutdatedItemWhenMultipleItemsSharePath()
        {
            var path = MockFactory.CreatePath();
            var first = MockFactory.CreateItem(path);
            var second = MockFactory.CreateItem(path);
            var sut = CreateRepository();

            sut.Add(Token, first);
            sut.Remove(Token, first);
            sut.Add(Token, second);
            sut.Remove(Token, second);

            var outdated = sut.DetachOutdatedItems();

            outdated.TryGetItem(path, out _).Should().BeFalse();
        }
    }

    private static FileSystemItemRepository CreateRepository()
    {
        return new FileSystemItemRepository();
    }
}