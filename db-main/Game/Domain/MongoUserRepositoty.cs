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
            //TODO: Это Find или Insert
            var usersEntity = userCollection.Find(x => x.Login == login);
            if (usersEntity.CountDocuments() > 0)
            {
                return usersEntity.First();
            }
            var userEntity = new UserEntity();
            userEntity.Login = login;
            Insert(userEntity);

            return userEntity;
        }

        public void Update(UserEntity user)
        {
            //TODO: Ищи в документации ReplaceXXX
            userCollection.ReplaceOne(new BsonDocument("_id", user.Id), user);
        }

        public void Delete(Guid id)
        {
            userCollection.DeleteOne(new BsonDocument("_id", id));
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            //TODO: Тебе понадобятся SortBy, Skip и Limit
            var a = userCollection.Find(new BsonDocument()).SortBy(x => x.Login).Skip((pageNumber - 1) * pageSize).Limit(pageSize).ToList();
            return new PageList<UserEntity>(a, userCollection.CountDocuments(new BsonDocument()), pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}