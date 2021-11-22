using System;
using MongoDB.Driver;

namespace Tests
{
    public static class TestMongoDatabase
    {
        public static IMongoDatabase Create()
        {
            const string connectionStr = "mongodb+srv://volkovalexandra11:P2VRhLtb2J4FMAg@cluster0.1xzo0.mongodb.net/myFirstDatabase";
            var mongoClient = new MongoClient(connectionStr);
            return mongoClient.GetDatabase("game-tests");
        }
    }
}