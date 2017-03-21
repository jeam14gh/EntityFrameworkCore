﻿using System.Threading;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.Metadata.Internal;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Storage
{
    public class MongoDbConnectionTests
    {
        private readonly Mock<IMongoDatabase> _mockMongoDatabase;
        private readonly Mock<IMongoClient> _mockMongoClient;
        private readonly Mock<IMongoCollection<Employee>> _mockEmployee;
        private readonly IModel _model;

        public MongoDbConnectionTests()
        {
            _model = GetModel();
            _mockMongoClient = MockMongoClient();
            _mockMongoDatabase = MockMongoDatabase();
            _mockEmployee = MockEmployee();
        }

        private IModel GetModel()
        {
            var model = new Model();
            model.Builder
                .MongoDb(ConfigurationSource.Explicit)
                .FromDatabase("zooDb")
                .InternalModelBuilder
                .Entity(typeof(Employee), ConfigurationSource.Explicit)
                .MongoDb(ConfigurationSource.Explicit)
                .FromCollection("employees");
            return model;
        }

        private Mock<IMongoClient> MockMongoClient()
        {
            var mockMongoClient = new Mock<IMongoClient>();
            mockMongoClient
                .Setup(mongoClient => mongoClient.GetDatabase("zooDb", It.IsAny<MongoDatabaseSettings>()))
                .Returns(() => _mockMongoDatabase.Object)
                .Verifiable();
            mockMongoClient
                .Setup(mongoClient => mongoClient.DropDatabase("zooDb", It.IsAny<CancellationToken>()))
                .Verifiable();
            return mockMongoClient;
        }

        private Mock<IMongoDatabase> MockMongoDatabase()
        {
            var mockMongoDatabase = new Mock<IMongoDatabase>();
            mockMongoDatabase
                .Setup(mongoDatabase => mongoDatabase.GetCollection<Employee>("employees", It.IsAny<MongoCollectionSettings>()))
                .Returns(() => _mockEmployee.Object)
                .Verifiable();
            return mockMongoDatabase;
        }

        private Mock<IMongoCollection<Employee>> MockEmployee()
            => new Mock<IMongoCollection<Employee>>();

        [Fact]
        public void Get_database_calls_mongo_client_get_database()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            Assert.Equal(_mockMongoDatabase.Object, mongoDbConnection.GetDatabase());
            _mockMongoClient
                .Verify(mongoClient => mongoClient.GetDatabase("zooDb", It.IsAny<MongoDatabaseSettings>()), Times.Once);
        }

        [Fact]
        public async Task Get_database_async_calls_mongo_client_get_database()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            Assert.Equal(_mockMongoDatabase.Object, await mongoDbConnection.GetDatabaseAsync());
            _mockMongoClient
                .Verify(mongoClient => mongoClient.GetDatabase("zooDb", It.IsAny<MongoDatabaseSettings>()), Times.Once);
        }

        [Fact]
        public void Drop_database_calls_mongo_client_drop_database()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            mongoDbConnection.DropDatabase();
            _mockMongoClient
                .Verify(mongoClient => mongoClient.DropDatabase("zooDb", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Drop_database_async_calls_mongo_client_drop_database_async()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            await mongoDbConnection.DropDatabaseAsync();
            _mockMongoClient
                .Verify(mongoClient => mongoClient.DropDatabaseAsync("zooDb", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Get_collection_calls_mongo_database_get_collection()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            Assert.Equal(_mockEmployee.Object, mongoDbConnection.GetCollection<Employee>());
            _mockMongoDatabase
                .Verify(mongoDatabase => mongoDatabase.GetCollection<Employee>("employees", It.IsAny<MongoCollectionSettings>()), Times.Once);
        }
    }
}