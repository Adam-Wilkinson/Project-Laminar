using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Synchronization;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Synchronization.UnitTests;

public class FileSystemDiscrepancyComputerTests
{
    public class ComputeFolderDiscrepancies
    {
        [Fact]
        public void ShouldReportDeletedWhenItemNoLongerExists()
        {
            var folder = MockFactory.CreateFolder();
            var fileSystem = CreateFileSystem();

            fileSystem.Exists(folder.Path)
                .Returns(false);

            var sut = CreateComputer(fileSystem);

            var result = sut.ComputeFolderDiscrepancies(folder).ToList();

            result.Should().ContainSingle()
                .Which.Should().Be(
                    FileSystemEvent.Deleted(folder.Path));
        }

        [Fact]
        public void ShouldReturnNoChangesForUninitializedFolder()
        {
            var folder = MockFactory.CreateFolder();
            folder.Contents.Returns((IReadOnlyList<IFileSystemItem>?)null);

            var fileSystem = CreateFileSystem();

            fileSystem.Exists(folder.Path)
                .Returns(true);

            var sut = CreateComputer(fileSystem);

            var result = sut.ComputeFolderDiscrepancies(folder).ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReportCreatedChildrenMissingFromFolder()
        {
            var folder = MockFactory.CreateFolder(contents: []);
            var newChildPath = MockFactory.CreatePath(name: "new-file.txt");

            var fileSystem = CreateFileSystem();

            fileSystem.Exists(folder.Path)
                .Returns(true);

            fileSystem.EnumerateChildren(folder.Path)
                .Returns([newChildPath]);

            var sut = CreateComputer(fileSystem);

            var result = sut.ComputeFolderDiscrepancies(folder).ToList();

            result.Should().ContainSingle()
                .Which.Should().Be(
                    FileSystemEvent.Created(newChildPath));
        }

        [Fact]
        public void ShouldNotReportExistingChildrenAsCreated()
        {
            var (child, _) = MockFactory.CreateFile();
            var folder = MockFactory.CreateFolder(contents: [child]);
            var folderPath = folder.Path;
            var childPath = child.Path;
            
            var fileSystem = CreateFileSystem();

            fileSystem.Exists(folderPath).Returns(true);
            fileSystem.Exists(childPath).Returns(true);

            fileSystem.EnumerateChildren(folderPath).Returns((IEnumerable<FileSystemPath>)[childPath]);

            var sut = CreateComputer(fileSystem);

            var result = sut.ComputeFolderDiscrepancies(folder).ToList();

            result.Should().BeEmpty();
        }

        [Fact]
        public void ShouldComputeNestedDiscrepancies()
        {
            var childFolder = MockFactory.CreateFolder();
            var root = MockFactory.CreateFolder(contents: [childFolder]);
            var rootPath = root.Path;
            var childPath = childFolder.Path;
            
            var fileSystem = CreateFileSystem();

            fileSystem.Exists(root.Path).Returns(true);

            fileSystem.EnumerateChildren(rootPath).Returns((IEnumerable<FileSystemPath>)[childPath]);

            fileSystem.Exists(childFolder.Path).Returns(false);

            var sut = CreateComputer(fileSystem);

            var result = sut.ComputeFolderDiscrepancies(root).ToList();

            result.Should().ContainSingle().Which.Should().Be(FileSystemEvent.Deleted(childFolder.Path));
        }

        [Fact]
        public void ShouldReportMultipleMissingChildren()
        {
            var folder = MockFactory.CreateFolder(contents: []);

            var firstPath = MockFactory.CreatePath(name: "first.txt");
            var secondPath = MockFactory.CreatePath(name: "second.txt");

            var fileSystem = CreateFileSystem();

            fileSystem.Exists(folder.Path)
                .Returns(true);

            fileSystem.EnumerateChildren(folder.Path).Returns([firstPath, secondPath]);

            var sut = CreateComputer(fileSystem);

            var result = sut.ComputeFolderDiscrepancies(folder).ToList();

            result.Should().BeEquivalentTo(
            [
                FileSystemEvent.Created(firstPath),
                FileSystemEvent.Created(secondPath)
            ]);
        }
    }

    private static FileSystemDiscrepancyComputer CreateComputer(
        IFileSystem? fileSystem = null)
    {
        return new FileSystemDiscrepancyComputer(
            fileSystem ?? CreateFileSystem());
    }

    private static IFileSystem CreateFileSystem()
    {
        return Substitute.For<IFileSystem>();
    }
}