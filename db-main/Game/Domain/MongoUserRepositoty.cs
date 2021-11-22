using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
            userCollection.Indexes.CreateOne(
                new BsonDocument("Login", 1), 
                new CreateIndexOptions { Unique = true }
                );
        }

        public UserEntity Insert(UserEntity user)
        {
            //TODO: Ищи в документации InsertXXX.
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            //TODO: Ищи в документации FindXXX
            var userEntity = userCollection.Find(x => x.Id == id).FirstOrDefault();
            return userEntity;
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var usersEntity = userCollection
                .Find(x => x.Login == login);
            if (usersEntity.CountDocuments() > 0)
            {
                return usersEntity.First();
            }
            var userEntity = new UserEntity
            {
                Login = login
            };
            Insert(userEntity);

            return userEntity;
        }

        public void Update(UserEntity user)
        {
            userCollection
                .ReplaceOne(new BsonDocument("_id", user.Id), user);
        }

        public void Delete(Guid id)
        {
            userCollection
                .DeleteOne(new BsonDocument("_id", id));
        }
        
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var userEntities = userCollection
                .Find(new BsonDocument())
                .SortBy(x => x.Login)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
            return new PageList<UserEntity>(userEntities, 
                userCollection.CountDocuments(new BsonDocument()), 
                pageNumber, 
                pageSize);
        }
        
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}