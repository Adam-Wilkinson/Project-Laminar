using FluentAssertions;
using Laminar.Contracts;
using Laminar.Contracts.UserData;
using NSubstitute;
using File = Laminar.Implementation.UserData.File;

namespace Laminar.Implementation.UnitTests.UserData.UnitTests;

public class FileTests
{
    private const string FileDirectory = "TestDirectory";
    private const string FileName = "TestContents.extension";
    private const string FilePath = FileDirectory + "\\" + FileName; 
    
    [Fact]
    public void ShouldUpdateContents_WhenContentsSet()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytes(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<string>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.GetParent(FilePath).Returns(new DirectoryInfo(FileDirectory));
        
        var sut = new File(mockFileSystem, FilePath);
        using var mon = sut.Monitor();

        sut.Contents = newFileContents;
        
        mockFileSystem.Received(1).WriteBytesAsync(FilePath, newFileContents, Arg.Any<CancellationToken>());
        mon.Should().Raise(nameof(IFile.ContentsChanged));
        Assert.Equal(newFileContents, sut.Contents);
        sut.Dispose();
    }
    
    [Fact]
    public void ShouldNotReadFile_WhenContentsSet()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytes(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<string>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.GetParent(FilePath).Returns(new DirectoryInfo(FileDirectory));
        mockFileSystem.GetFileName(FilePath).Returns(FileName);
        
        var sut = new File(mockFileSystem, FilePath);
        using var mon = sut.Monitor();

        sut.Contents = newFileContents;
        mockFileWatcher.Changed += Raise.Event<FileSystemEventHandler>(new FileSystemEventArgs(WatcherChangeTypes.Changed, FileDirectory, FileName));

        mockFileSystem.Received(1).ReadBytes(FilePath);
        sut.Dispose();
    }

    [Fact]
    public void ShouldUpdateContents_WhenFileChanged()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytes(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<string>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.GetParent(FilePath).Returns(new DirectoryInfo(FileDirectory));
        mockFileSystem.GetFileName(FilePath).Returns(FileName);
    
        var sut = new File(mockFileSystem, FilePath);
        using var mon = sut.Monitor();
        
        mockFileSystem.ReadBytes(FilePath).Returns(newFileContents);
        mockFileWatcher.Changed += Raise.Event<FileSystemEventHandler>(new FileSystemEventArgs(WatcherChangeTypes.Changed, FileDirectory, FileName));
        
        mon.Should().Raise(nameof(IFile.ContentsChanged));
        Assert.Equal(newFileContents, sut.Contents);
        sut.Dispose();
    }
    
    [Fact]
    public void ShouldNotUpdateContents_WhenDifferentFileChanged()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytes(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<string>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.GetParent(FilePath).Returns(new DirectoryInfo(FileDirectory));
        mockFileSystem.GetFileName(FilePath).Returns(FileName);
    
        var sut = new File(mockFileSystem, FilePath);
        using var mon = sut.Monitor();
        
        mockFileSystem.ReadBytes(FilePath).Returns(newFileContents);
        mockFileWatcher.Changed += Raise.Event<FileSystemEventHandler>(new FileSystemEventArgs(WatcherChangeTypes.Changed, FileDirectory, "another file.txt"));
        
        mon.Should().NotRaise(nameof(IFile.ContentsChanged));
        Assert.Equal(initialFileContents, sut.Contents);
        sut.Dispose();
    }
}