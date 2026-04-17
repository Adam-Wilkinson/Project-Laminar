using System.Text;
using System.Text.Json;
using FluentAssertions;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.Implementation.UserData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Laminar.Implementation.UnitTests.UserData.UnitTests;

public class PersistentDataStoreTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly ILogger<IPersistentDataStore> Logger = Substitute.For<ILogger<IPersistentDataStore>>();
    private static readonly ILogger<JsonPersistentDataTranscoder> TranscoderLogger = Substitute.For<ILogger<JsonPersistentDataTranscoder>>();

    public class GetOrCreateChild
    {
        [Fact]
        public void ShouldUseExisting_WhenChildExists()
        {
            const string childDatastoreKey = "Test Key";

            var serializerMock = Substitute.For<ISerializer>();
            var sut = new PersistentDataStore<JsonElement>(serializerMock,
                new JsonPersistentDataTranscoder(JsonOptions, TranscoderLogger), Logger);
            
            var childOne = sut.GetOrCreateChild(childDatastoreKey);
            var childTwo = sut.GetOrCreateChild(childDatastoreKey);
            
            childOne.Should().BeSameAs(childTwo);
        }

        [Fact]
        public void ShouldThrowError_WhenChildIncorrect()
        {
            const string childDatastoreKey = "Test Key";

            var serializerMock = Substitute.For<ISerializer>();
            var sut = new PersistentDataStore<JsonElement>(serializerMock,
                new JsonPersistentDataTranscoder(JsonOptions, TranscoderLogger), Logger);
            
            sut.InitializeDefaultValue(childDatastoreKey, 5, typeof(int));

            sut.Invoking(x => x.GetOrCreateChild(childDatastoreKey))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ShouldUpdate_WhenChildUpdated()
        {
            const string childDataStoreKey = "Child Key";
            const string testValueKey = "Test Key";
            const double defaultValue = 5.0;
            
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());
            serializerMock.DeserializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());

            serializerMock.GetSerializedType(typeof(PersistentDataStore<JsonElement>)).Returns(typeof(Dictionary<string, JsonElement>));
            serializerMock.SerializeObject(Arg.Any<PersistentDataStore<JsonElement>>(), typeof(PersistentDataStore<JsonElement>))
                .Returns(ci => ci.Arg<PersistentDataStore<JsonElement>>().Serialize());
            serializerMock.DeserializeObject(Arg.Any<Dictionary<string, JsonElement>>(), typeof(PersistentDataStore<JsonElement>), Arg.Any<object>()).Returns(
                ci => PersistentDataStore<JsonElement>.Deserialize(ci.Arg<Dictionary<string, JsonElement>>(),  ci.Arg<object>()));
            
            var sut = new PersistentDataStore<JsonElement>(serializerMock,
                new JsonPersistentDataTranscoder(JsonOptions, TranscoderLogger), Logger);
            
            var child = sut.GetOrCreateChild(childDataStoreKey);

            child.InitializeDefaultValue(testValueKey, defaultValue);

            string expectedString = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    [childDataStoreKey] = new Dictionary<string, double>
                    {
                        [testValueKey] = defaultValue,
                    }
                },
                JsonOptions);
            
            Assert.Equal(Encoding.UTF8.GetBytes(expectedString), sut.RawData);
        }
    }
    
    public class InitializeDefault
    {
        public static TheoryData<Dictionary<string, object>> InitialPersistentData => 
        [
            new(),
            new() { ["Test Item One"] = false },
            new() { ["Test Item One"] = "A string", ["Test Item Two"] = 60 }
        ];
        
        [Theory, MemberData(nameof(InitialPersistentData))]
        public void ShouldAddItem_WhenItemDoesntExist(Dictionary<string, object> initialData)
        {
            const string testValueKey = "Test Key";
            const double testValue = 5.0;

            var fileData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(initialData, JsonOptions));
                
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(testValue).Returns(testValue);

            var sut = new PersistentDataStore<JsonElement>(serializerMock, new JsonPersistentDataTranscoder(JsonOptions, TranscoderLogger), Logger)
                {
                    RawData = fileData
                };
            
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
            
            var fileData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(initialData, JsonOptions));
            
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(Arg.Any<double>()).Returns(ci => ci.Arg<double>());
            serializerMock.DeserializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());

            var sut = new PersistentDataStore<JsonElement>(serializerMock, new JsonPersistentDataTranscoder(JsonOptions, TranscoderLogger), Logger)
            {
                RawData = fileData
            };
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

            var rawData = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new Dictionary<string, double> { [testValueKey] = initialValue },
                    JsonOptions));
            
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(Arg.Any<double>()).Returns(ci => ci.Arg<double>());
            serializerMock.DeserializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());

            var sut = new PersistentDataStore<JsonElement>(serializerMock, new JsonPersistentDataTranscoder(JsonOptions, TranscoderLogger), Logger)
            {
                RawData = rawData
            };
            
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

            var rawData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, double> { [testValueKey] = initialValue }, JsonOptions));
                
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(Arg.Any<double>()).Returns(ci => ci.Arg<double>());
            serializerMock.SerializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());
            serializerMock.DeserializeObject(Arg.Any<double>(), typeof(double)).Returns(ci => ci.Arg<double>());

            var sut = new PersistentDataStore<JsonElement>(serializerMock, new JsonPersistentDataTranscoder(JsonOptions, TranscoderLogger), Logger)
            {
                RawData = rawData
            };
            sut.InitializeDefaultValue(testValueKey, initialValue);

            sut.SetItem(testValueKey, newValue);

            var expectedFileContents =
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(new Dictionary<string, double> { [testValueKey] = newValue },
                        JsonOptions));
            
            Assert.Equal(expectedFileContents, sut.RawData);
        }
        
        [Fact]
        public void ShouldReturnError_WhenItemDoesntExist()
        {
            const string testValueKey = "Test Key";
            const double testValue = 5.0;
            
            var serializerMock = Substitute.For<ISerializer>();
            serializerMock.GetSerializedType(typeof(double)).Returns(typeof(double));
            serializerMock.SerializeObject(testValue).Returns(testValue);
            
            var sut = new PersistentDataStore<JsonElement>(serializerMock, new JsonPersistentDataTranscoder(JsonOptions, TranscoderLogger), Logger);
            
            Assert.Equal(DataIoStatus.DataNotFound, sut.SetItem(testValueKey, testValue).Status);
        }
    }
}