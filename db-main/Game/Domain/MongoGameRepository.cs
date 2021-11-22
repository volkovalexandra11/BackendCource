using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace Game.Domain
{
    // TODO Сделать по аналогии с MongoUserRepository
    public class MongoGameRepository : IGameRepository
    {
        public const string CollectionName = "games";
        private readonly IMongoCollection<GameEntity> gameCollection;

        public MongoGameRepository(IMongoDatabase db)
        {
            gameCollection = db.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            gameCollection.InsertOne(game);
            return game;
        }

        public GameEntity FindById(Guid gameId)
        {
            return gameCollection
                .Find(game => game.Id == gameId)
                .FirstOrDefault();
        }

        public void Update(GameEntity game)
        {
            gameCollection
                .ReplaceOne(g => g.Id == game.Id, game);
        }
        
        public IList<GameEntity> FindWaitingToStart(int limit)
        {
            var games = gameCollection
                .Find(game => game.Status == GameStatus.WaitingToStart)
                .Limit(limit)
                .ToList();

            return games.ToList();
        }

        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            var gameFromBase = FindById(game.Id);
            if (gameFromBase is null || gameFromBase.Status != GameStatus.WaitingToStart)
                return false;
            var updatedGame = new GameEntity(
                game.Id, 
                GameStatus.Playing,
                game.TurnsCount,
                game.CurrentTurnIndex, 
                game.Players.ToList()
            );

            gameCollection.ReplaceOne(g => g.Id == game.Id, updatedGame);

            return true;
        }
    }
}