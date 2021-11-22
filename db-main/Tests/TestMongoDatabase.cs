using System;
using MongoDB.Driver;

namespace Tests
{
    public static class TestMongoDatabase
    {
        public static IMongoDatabase Create()
        {
            const string connectionStr = "mongodb+srv://volkovalexandra11:P2VRhLtb2J4FMAg@cluster0.1xzo0.mongodb.net/myFirstDatabase?retryWrites=true&w=majority";
            var mongoConnectionString = Environment.GetEnvironmentVariable(connectionStr) ?? "mongodb://localhost:27017";
            var mongoClient = new MongoClient(mongoConnectionString);
            return mongoClient.GetDatabase("game-tests");
        }
    }
}