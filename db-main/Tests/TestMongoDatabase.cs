using System;
using MongoDB.Driver;

namespace Tests
{
    public static class TestMongoDatabase
    {
        public static IMongoDatabase Create()
        {
            var mongoConnectionString = Environment.GetEnvironmentVariable("mongodb+srv://kwcdb:kwcdb16112021@cluster0.zkvhx.mongodb.net/game-tests?retryWrites=true&w=majority")
                                        ?? "mongodb://localhost:27017";
            var mongoClient = new MongoClient(mongoConnectionString);
            return mongoClient.GetDatabase("game-tests");
        }
    }
}