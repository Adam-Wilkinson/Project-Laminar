using System.Text;
using System.Text.Json;
using Laminar.Contracts;
using Laminar.Domain.DataManagement;
using Laminar.Implementation.UserData;
using Laminar.PluginFramework.Serialization;
using NSubstitute;

namespace Laminar.Implementation.UnitTests.UserData.UnitTests;

public class PersistentDataStoreTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    
    public class InitializeDefault
    {
        public static IEnumerable<object[]> InitialPersistentData => new List<object[]>
        {
            new object[] { new Dictionary<string, object>() },
            new object[] { new Dictionary<string, object> { ["Test Item One"] = false }},
            new object[] { new Dictionary<string, object> { ["Test Item One"] = "A string", ["Test Item Two"] = 60 }}
        };
        
        [Theory, MemberData(nameof(InitialPersistentData))]
        public void ShouldAddItem_WhenItemDoesntExist(Dictionary<string, object> initialData)
        {
            const string testValueKey = "Test Key";
            const double testValue = 5.0;
            
            var fileMock = Substitute.For<IFile>();
            fileMock.Contents.Returns(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(initialData, JsonOptions )));
            
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(testValue).Returns(testValue);

            var sut = new PersistentDataStore<JsonElement>(fileMock, serializerMock, new JsonPersistentDataTranscoder(JsonOptions));
            sut.InitializeDefaultValue(testValueKey, testValue);

            Assert.Equal(testValue, sut.GetItem<double>(testValueKey).Result);
        }
        
        [Theory, MemberData(nameof(InitialPersistentData))]
        public void ShouldTakeSavedItem_WhenItemExists(Dictionary<string, object> initialData)
        {
            const string testValueKey = "Test Key";
            const double defaultValue = 5.0;
            const double savedValue = 10.0;

            initialData[testValueKey] = savedValue;
            
            var fileMock = Substitute.For<IFile>();
            fileMock.Contents.Returns(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(initialData, JsonOptions )));
            
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(Arg.Any<double>()).Returns(ci => ci.Arg<double>());
            serializerMock.DeserializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());

            var sut = new PersistentDataStore<JsonElement>(fileMock, serializerMock, new JsonPersistentDataTranscoder(JsonOptions));
            sut.InitializeDefaultValue(testValueKey, defaultValue);

            Assert.Equal(savedValue, sut.GetItem<double>(testValueKey).Result);
        }
    }

    public class SetItem
    {
        [Fact]
        public void ShouldSetItem_WhenItemInitialized()
        {
            const string testValueKey = "Test Key";
            const double initialValue = 5.0;
            const double newValue = 10.0;
            
            var fileMock = Substitute.For<IFile>();
            fileMock.Contents.Returns(Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new Dictionary<string, double> { [testValueKey] = initialValue }, JsonOptions)));
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(Arg.Any<double>()).Returns(ci => ci.Arg<double>());
            serializerMock.DeserializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());
            
            var sut = new PersistentDataStore<JsonElement>(fileMock, serializerMock, new JsonPersistentDataTranscoder(JsonOptions));
            sut.InitializeDefaultValue(testValueKey, initialValue);

            sut.SetItem(testValueKey, newValue);
            
            Assert.Equal(newValue, sut.GetItem<double>(testValueKey).Result);
        }

        [Fact]
        public void ShouldUpdateFile_WhenItemSet()
        {
            const string testValueKey = "Test Key";
            const double initialValue = 5.0;
            const double newValue = 10.0;
            
            var fileMock = Substitute.For<IFile>();
            fileMock.Contents.Returns(Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new Dictionary<string, double> { [testValueKey] = initialValue }, JsonOptions)));
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(Arg.Any<double>()).Returns(ci => ci.Arg<double>());
            serializerMock.SerializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());
            serializerMock.DeserializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());
            
            var sut = new PersistentDataStore<JsonElement>(fileMock, serializerMock, new JsonPersistentDataTranscoder(JsonOptions));
            sut.InitializeDefaultValue(testValueKey, initialValue);

            sut.SetItem(testValueKey, newValue);

            var expectedFileContents =
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(new Dictionary<string, double> { [testValueKey] = newValue },
                        JsonOptions));
            
            Assert.Equal(expectedFileContents, fileMock.Contents);
        }
        
        [Fact]
        public void ShouldReturnError_WhenItemDoesntExist()
        {
            const string testValueKey = "Test Key";
            const double testValue = 5.0;
            
            var fileMock = Substitute.For<IFile>();
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(testValue).Returns(testValue);
            
            var sut = new PersistentDataStore<JsonElement>(fileMock, serializerMock, new JsonPersistentDataTranscoder(JsonOptions));
            
            Assert.Equal(DataIoStatus.DataNotFound, sut.SetItem(testValueKey, testValue).Status);
        }
    }
}