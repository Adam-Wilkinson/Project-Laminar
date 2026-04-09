using FluentAssertions;
using Laminar.Contracts.UserData;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using File = Laminar.Implementation.UserData.File;

namespace Laminar.Implementation.UnitTests.UserData.UnitTests;

public class FileTests
{
    private const string FileName = "TestContents.extension";
    private static readonly FileSystemPath FileDirectory = new("TestDirectory");
    private static readonly FileSystemPath FilePath = FileDirectory.ChildPath(FileName); 
    
    private readonly ILogger<File> _logger = Substitute.For<ILogger<File>>();
    
    [Fact]
    public void ShouldUpdateContents_WhenContentsSet()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytes(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<FileSystemPath>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.Exists(FilePath).Returns(true);
        
        using var sut = new File(mockFileSystem, FilePath, _logger);
        using var mon = sut.Monitor();

        sut.Contents = newFileContents;
        sut.CheckAccess();
        
        mockFileSystem.Received(1).WriteBytesAsync(FilePath, newFileContents, Arg.Any<CancellationToken>());
        mon.Should().Raise(nameof(IFile.ContentsChanged));
        Assert.Equal(newFileContents, sut.Contents);
    }
    
    [Fact]
    public void ShouldNotReadFile_WhenContentsSet()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytes(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<FileSystemPath>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.Exists(FilePath).Returns(true);
        
        using var sut = new File(mockFileSystem, FilePath, _logger);
        using var mon = sut.Monitor();

        sut.Contents = newFileContents;
        mockFileWatcher.Changed += Raise.Event<FileSystemEventHandler>(new FileSystemEventArgs(WatcherChangeTypes.Changed, FileDirectory.ToString(), FileName));

        sut.CheckAccess();
        
        mockFileSystem.Received(1).ReadBytes(FilePath);
    }

    [Fact]
    public void ShouldUpdateContents_WhenFileChanged()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        var mockFileStream = Substitute.For<IFileStream>();
        mockFileSystem.ReadBytes(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<FileSystemPath>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.CreateFile(FilePath).Returns(mockFileStream);
        
        using var sut = new File(mockFileSystem, FilePath, _logger);
        using var mon = sut.Monitor();
        
        mockFileSystem.ReadBytes(FilePath).Returns(newFileContents);
        mockFileWatcher.Changed += Raise.Event<FileSystemEventHandler>(new FileSystemEventArgs(WatcherChangeTypes.Changed, FileDirectory.ToString(), FileName));
        sut.CheckAccess();
        
        mon.Should().Raise(nameof(IFile.ContentsChanged));
        Assert.Equal(newFileContents, sut.Contents);
    }
    
    [Fact]
    public void ShouldNotUpdateContents_WhenDifferentFileChanged()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytes(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<FileSystemPath>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.Exists(FilePath).Returns(true);
    
        using var sut = new File(mockFileSystem, FilePath, _logger);
        using var mon = sut.Monitor();
        
        mockFileSystem.ReadBytes(FilePath).Returns(newFileContents);
        mockFileWatcher.Changed += Raise.Event<FileSystemEventHandler>(new FileSystemEventArgs(WatcherChangeTypes.Changed, FileDirectory.ToString(), "another file.txt"));
        
        mon.Should().NotRaise(nameof(IFile.ContentsChanged));
        Assert.Equal(initialFileContents, sut.Contents);
    }
}