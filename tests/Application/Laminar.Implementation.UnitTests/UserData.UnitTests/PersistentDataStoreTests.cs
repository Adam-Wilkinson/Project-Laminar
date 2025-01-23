using Laminar.Contracts.UserData;
using Laminar.Implementation.UserData;
using Laminar.PluginFramework.Serialization;
using Moq;

namespace Laminar.Implementation.UnitTests.UserData.UnitTests;

public class PersistentDataStoreTests
{
    private PersistentDataStore _sut = new(new Mock<IFileSystem>().Object, new Mock<ISerializer>().Object, new Mock<IPersistentDataTranscoder>().Object, "TestDataPath");
}