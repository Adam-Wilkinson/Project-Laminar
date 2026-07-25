using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Implementation.Storage.FileExplorer;
using Laminar.Implementation.Storage.FileExplorer.Graph;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Graph.UnitTests;

public class FileSystemItemFactoryTests
{
    public class CreateFile
    {
        private const string FileName = "File.txt";
        
        [Fact]
        public void ShouldCreateFile()
        {
            var parent = MockFactory.CreateFolder();
            var data = MockFactory.CreateItemData();
            var dataFactory = CreateDataFactory(data);
            var sut = CreateFactory(dataFactory: dataFactory);

            var result = sut.CreateFile(parent, FileName);

            result.Should().BeOfType<FileSystemFile>();
        }

        [Fact]
        public void ShouldInitializePersistentData()
        {
            var parent = MockFactory.CreateFolder();
            var data = MockFactory.CreateItemData();
            var name = MockFactory.CreateDataPoint<string>(FileName);
            var isFolder = MockFactory.CreateDataPoint<bool>(false);
            data[IFileSystemItemFactory.PersistenceNameKey].Returns(name);
            data[IFileSystemItemFactory.PersistenceIsFolderKey].Returns(isFolder);

            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            sut.CreateFile(parent, FileName);

            name.Received(1).GetValueOrInitialize(FileName);
            isFolder.Received(1).GetValueOrInitialize(false);
        }
    }

    public class CreateFolder
    {
        private const string FolderName = "Folder";
        
        [Fact]
        public void ShouldCreateFolder()
        {
            var parent = MockFactory.CreateFolder();
            var data = MockFactory.CreateItemData();
            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            var result = sut.CreateFolder(parent, FolderName);

            result.Should().BeOfType<FileSystemFolder>();
        }

        [Fact]
        public void ShouldInitializePersistentData()
        {
            var parent = MockFactory.CreateFolder();
            var data = MockFactory.CreateItemData();
            var name = MockFactory.CreateDataPoint<string>(FolderName);
            var isFolder = MockFactory.CreateDataPoint<bool>(true);
            data[IFileSystemItemFactory.PersistenceNameKey].Returns(name);
            data[IFileSystemItemFactory.PersistenceIsFolderKey].Returns(isFolder);

            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            sut.CreateFolder(parent, FolderName);

            name.Received(1).GetValueOrInitialize(FolderName);
            isFolder.Received(1).GetValueOrInitialize(true);
        }
    }

    public class CreateRootFolder
    {
        private const string RootName = "Root";
        
        [Fact]
        public void ShouldCreateRootFolder()
        {
            var path = MockFactory.CreatePath(name: RootName);
            var data = MockFactory.CreateItemData();
            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            var result = sut.CreateRootFolder(path);

            result.Should().BeOfType<FileSystemRootFolder>();
        }

        [Fact]
        public void ShouldInitializePersistentData()
        {
            var data = MockFactory.CreateItemData();
            var name = MockFactory.CreateDataPoint<string>(RootName);
            var isFolder = MockFactory.CreateDataPoint<bool>(true);
            data[IFileSystemItemFactory.PersistenceNameKey].Returns(name);
            data[IFileSystemItemFactory.PersistenceIsFolderKey].Returns(isFolder);

            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            sut.CreateRootFolder(RootName);

            name.Received(1).GetValueOrInitialize(RootName);
            isFolder.Received(1).GetValueOrInitialize(true);
        }
    }

    public class CreateFromPersistentData
    {
        [Fact]
        public void ShouldCreateFolderWhenPersistentDataRepresentsFolder()
        {
            var parent = MockFactory.CreateFolder();
            var persistentData = MockFactory.CreateItemData("Folder Name", true);

            var sut = CreateFactory();

            var result = sut.CreateFromPersistentData(parent, persistentData);

            result.Should().BeOfType<FileSystemFolder>();
        }

        [Fact]
        public void ShouldCreateFileWhenPersistentDataRepresentsFile()
        {
            var parent = MockFactory.CreateFolder();
            var persistentData = MockFactory.CreateItemData("File Name", false);

            var sut = CreateFactory();

            var result = sut.CreateFromPersistentData(parent, persistentData);

            result.Should().BeOfType<FileSystemFile>();
        }
    }

    private static FileSystemItemFactory CreateFactory(IEncodableDataFactory? dataFactory = null)
    {
        return new FileSystemItemFactory(
            CreateServiceProvider(),
            dataFactory ?? CreateDataFactory(MockFactory.CreateItemData()));
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(Substitute.For<IFileSystem>())
            .AddSingleton(Substitute.For<IFileSystemGraph>())
            .AddSingleton(Substitute.For<IPersistentDataManager>())
            .AddSingleton(Substitute.For<IFileSystemMonitor>())
            .BuildServiceProvider();

        return provider;
    }

    private static IEncodableDataFactory CreateDataFactory(IPersistentDictionary dictionary)
    {
        var factory = Substitute.For<IEncodableDataFactory>();
        factory.GetEncodableData<IPersistentDictionary>().Returns(dictionary);
        return factory;
    }
}