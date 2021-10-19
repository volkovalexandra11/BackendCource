using System.Collections.Generic;
using BadNews.Repositories.News;

namespace BadNews.Models.News
{
    public class IndexModel
    {
        public IList<NewsArticle> FeaturedArticles { get; set; }
        public IList<NewsArticle> PageArticles { get; set; }
        public int PageIndex { get; set; }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
        public int? Year { get; set; }
    }
}
