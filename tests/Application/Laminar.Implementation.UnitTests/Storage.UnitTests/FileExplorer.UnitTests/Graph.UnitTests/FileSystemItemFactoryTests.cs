using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer;
using Laminar.Implementation.Storage.FileExplorer.Graph;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.UnitTests.Storage.UnitTests.FileExplorer.UnitTests.Graph.UnitTests;

public class FileSystemItemFactoryTests
{
    public class CreateFile
    {
        [Fact]
        public void ShouldCreateFile()
        {
            var parent = CreateParent();
            var data = CreatePersistentDictionary();
            var dataFactory = CreateDataFactory(data);
            var sut = CreateFactory(dataFactory: dataFactory);

            var result = sut.CreateFile(parent, "File.txt");

            result.Should().BeOfType<FileSystemFile>();
        }

        [Fact]
        public void ShouldInitializePersistentData()
        {
            var parent = CreateParent();
            var data = CreatePersistentDictionary();
            var name = CreateValue<string>();
            var isFolder = CreateValue<bool>();
            data[IFileSystemItemFactory.PersistenceNameKey].Returns(name);
            data[IFileSystemItemFactory.PersistenceIsFolderKey].Returns(isFolder);

            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            sut.CreateFile(parent, "File.txt");

            name.Received(1).GetValueOrInitialize("File.txt");
            isFolder.Received(1).GetValueOrInitialize(false);
        }
    }

    public class CreateFolder
    {
        [Fact]
        public void ShouldCreateFolder()
        {
            var parent = CreateParent();
            var data = CreatePersistentDictionary();
            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            var result = sut.CreateFolder(parent, "Folder");

            result.Should().BeOfType<FileSystemFolder>();
        }

        [Fact]
        public void ShouldInitializePersistentData()
        {
            var parent = CreateParent();
            var data = CreatePersistentDictionary();
            var name = CreateValue<string>();
            var isFolder = CreateValue<bool>();
            data[IFileSystemItemFactory.PersistenceNameKey].Returns(name);
            data[IFileSystemItemFactory.PersistenceIsFolderKey].Returns(isFolder);

            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            sut.CreateFolder(parent, "Folder");

            name.Received(1).GetValueOrInitialize("Folder");
            isFolder.Received(1).GetValueOrInitialize(true);
        }
    }

    public class CreateRootFolder
    {
        [Fact]
        public void ShouldCreateRootFolder()
        {
            var path = "Root";
            var data = CreatePersistentDictionary();
            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            var result = sut.CreateRootFolder(path);

            result.Should().BeOfType<FileSystemRootFolder>();
        }

        [Fact]
        public void ShouldInitializePersistentData()
        {
            var path = "Root";
            var data = CreatePersistentDictionary();
            var name = CreateValue<string>();
            var isFolder = CreateValue<bool>();
            data[IFileSystemItemFactory.PersistenceNameKey].Returns(name);
            data[IFileSystemItemFactory.PersistenceIsFolderKey].Returns(isFolder);

            var sut = CreateFactory(dataFactory: CreateDataFactory(data));

            sut.CreateRootFolder(path);

            name.Received(1).GetValueOrInitialize("Root");
            isFolder.Received(1).GetValueOrInitialize(true);
        }
    }

    public class CreateFromPersistentData
    {
        [Fact]
        public void ShouldCreateFolderWhenPersistentDataRepresentsFolder()
        {
            var parent = CreateParent();
            var isFolderValue = Substitute.For<IPersistentValue<bool>>();
            isFolderValue.Value.Returns(true);
            var persistent = CreatePersistentDictionary();
            persistent[IFileSystemItemFactory.PersistenceIsFolderKey]
                .GetValue<bool>()
                .Returns(isFolderValue);

            var sut = CreateFactory();

            var result = sut.CreateFromPersistentData(parent, persistent);

            result.Should().BeOfType<FileSystemFolder>();
        }

        [Fact]
        public void ShouldCreateFileWhenPersistentDataRepresentsFile()
        {
            var parent = CreateParent();
            var isFolderValue = Substitute.For<IPersistentValue<bool>>();
            isFolderValue.Value.Returns(false);
            var persistent = CreatePersistentDictionary();
            persistent[IFileSystemItemFactory.PersistenceIsFolderKey]
                .GetValue<bool>()
                .Returns(isFolderValue);

            var sut = CreateFactory();

            var result = sut.CreateFromPersistentData(parent, persistent);

            result.Should().BeOfType<FileSystemFile>();
        }
    }

    private static FileSystemItemFactory CreateFactory(IEncodableDataFactory? dataFactory = null)
    {
        return new FileSystemItemFactory(
            CreateServiceProvider(),
            dataFactory ?? CreateDataFactory(CreatePersistentDictionary()));
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

    private static IPersistentDictionary CreatePersistentDictionary()
    {
        var dictionary = Substitute.For<IPersistentDictionary>();

        var nameValue = CreateValue<string>();
        var isFolderValue = CreateValue<bool>();
        
        dictionary[IFileSystemItemFactory.PersistenceNameKey].Returns(nameValue);
        dictionary[IFileSystemItemFactory.PersistenceIsFolderKey].Returns(isFolderValue);

        return dictionary;
    }

    private static IPersistentDataPoint CreateValue<T>() where T : notnull
    {
        var value = Substitute.For<IPersistentValue<T>>();
        var point = Substitute.For<IPersistentDataPoint>();
        point.GetValue<T>().Returns(value);
        return point;
    }

    private static IFileSystemFolder CreateParent()
    {
        var parent = Substitute.For<IFileSystemFolder>();
        parent.Path.Returns(new FileSystemPath("ParentPath"));
        return parent;
    }
}