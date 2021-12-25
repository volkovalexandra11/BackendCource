using System;
using System.Collections.Generic;

namespace BadNews.Repositories.Comments
{
    public class CommentsRepository
    {
        public IReadOnlyCollection<Comment> GetComments(Guid newsId)
        {
            return new[]
            {
                new Comment("Вася", "Здесь был Вася!"),
                new Comment("Воробушек", "ЧИК-ЧИРИК"),
            };
        }
    }
}