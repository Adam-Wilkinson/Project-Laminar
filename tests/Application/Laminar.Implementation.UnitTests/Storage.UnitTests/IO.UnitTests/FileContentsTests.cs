using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.IO;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.IO.UnitTests;

public class FileContentsTests
{
    private const string FileName = "TestContents.extension";
    private static readonly FileSystemPath FileDirectory = new("TestDirectory");
    private static readonly FileSystemPath FilePath = FileDirectory.ChildPath(FileName); 
    
    private readonly ILogger<FileContents> _logger = Substitute.For<ILogger<FileContents>>();
    
    [Fact]
    public async Task ShouldUpdateContents_WhenContentsSet()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytesAsync(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<FileSystemPath>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.Exists(FilePath).Returns(true);
        
        using var sut = new FileContents(mockFileSystem, FilePath, _logger);
        using var mon = sut.Monitor();

        sut.Contents = newFileContents;

        await sut.WaitForPendingOperations();
        await mockFileSystem.Received(1).WriteBytes(FilePath, newFileContents, Arg.Any<CancellationToken>());
        mon.Should().Raise(nameof(IFileContents.ContentsChanged));
        Assert.Equal(newFileContents, sut.Contents);
    }
    
    [Fact]
    public async Task ShouldNotReadFile_WhenContentsSet()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];
    
        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytesAsync(FilePath, Arg.Any<CancellationToken>()).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<FileSystemPath>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.Exists(FilePath).Returns(true);
        
        using var sut = new FileContents(mockFileSystem, FilePath, _logger);
        using var mon = sut.Monitor();
    
        sut.Contents = newFileContents;
        mockFileSystem.ReadBytesAsync(FilePath, Arg.Any<CancellationToken>()).Returns(newFileContents);
        mockFileWatcher.Changed += Raise.Event<FileSystemEventHandler>(new FileSystemEventArgs(WatcherChangeTypes.Changed, FileDirectory.ToString(), FileName));

        await sut.WaitForPendingOperations();
        
        mon.Should().Raise(nameof(IFileContents.ContentsChanged)).Should().HaveCount(1);
    }

    [Fact]
    public async Task ShouldUpdateContents_WhenFileChanged()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        var mockFileStream = Substitute.For<IFileStream>();
        mockFileSystem.ReadBytesAsync(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<FileSystemPath>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.CreateFile(FilePath).Returns(mockFileStream);
        
        using var sut = new FileContents(mockFileSystem, FilePath, _logger);
        using var mon = sut.Monitor();
        
        mockFileSystem.ReadBytesAsync(FilePath, Arg.Any<CancellationToken>()).Returns(newFileContents);
        mockFileWatcher.Changed += Raise.Event<FileSystemEventHandler>(new FileSystemEventArgs(WatcherChangeTypes.Changed, FileDirectory.ToString(), FileName));

        await sut.WaitForPendingOperations();
        
        mon.Should().Raise(nameof(IFileContents.ContentsChanged));
        Assert.Equal(newFileContents, sut.Contents);
    }
    
    [Fact]
    public void ShouldNotUpdateContents_WhenDifferentFileChanged()
    {
        byte[] initialFileContents = [1, 2, 3, 4, 5];
        byte[] newFileContents = [6, 7, 8, 9, 10];

        var mockFileWatcher = Substitute.For<IFileWatcher>();
        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.ReadBytesAsync(FilePath).Returns(initialFileContents);
        mockFileSystem.ReadBytes(FilePath).Returns(initialFileContents);
        mockFileSystem.CreateFileWatcher(Arg.Any<FileSystemPath>(), Arg.Any<string>()).Returns(mockFileWatcher);
        mockFileSystem.Exists(FilePath).Returns(true);
    
        using var sut = new FileContents(mockFileSystem, FilePath, _logger);
        using var mon = sut.Monitor();
        
        mockFileSystem.ReadBytesAsync(FilePath).Returns(newFileContents);
        mockFileWatcher.Changed += Raise.Event<FileSystemEventHandler>(new FileSystemEventArgs(WatcherChangeTypes.Changed, FileDirectory.ToString(), "another file.txt"));
        
        mon.Should().NotRaise(nameof(IFileContents.ContentsChanged));
        Assert.Equal(initialFileContents, sut.Contents);
    }
}