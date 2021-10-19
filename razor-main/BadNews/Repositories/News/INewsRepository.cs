using System;
using System.Collections.Generic;

namespace BadNews.Repositories.News
{
    public interface INewsRepository
    {
        IList<NewsArticle> GetArticles(Func<NewsArticle, bool> predicate = null);
        IList<int> GetYearsWithArticles();
        NewsArticle GetArticleById(Guid id);
        Guid CreateArticle(NewsArticle article);
        void DeleteArticleById(Guid id);
    }
}
