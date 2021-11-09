using System.Collections.Generic;
using BadNews.Repositories.News;

namespace BadNews.Models.News
{
    public class IndexModel
    {
        public IList<NewsArticle> FeaturedArticles { get; init; }
        public IList<NewsArticle> PageArticles { get; init; }
        public int PageIndex { get; init; }
        public bool IsFirst { get; init; }
        public bool IsLast { get; init; }
        public int? Year { get; init; }
    }
}
