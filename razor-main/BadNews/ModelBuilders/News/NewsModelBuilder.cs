using BadNews.Repositories.News;
using BadNews.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BadNews.ModelBuilders.News
{
    public class NewsModelBuilder : INewsModelBuilder
    {
        private const int PageSize = 5;
        private readonly INewsRepository newsRepository;

        public NewsModelBuilder(INewsRepository newsRepository)
        {
            this.newsRepository = newsRepository;
        }

        public IndexModel BuildIndexModel(int pageIndex, bool useFeaturedArticles, int? year)
        {
            var articles = newsRepository
                .GetArticles(article => !year.HasValue || article.Date.Year == year.Value)
                .OrderByDescending(it => it.Date)
                .ToList();

            var featuredArticles = !year.HasValue && useFeaturedArticles
                ? articles.Where(a => a.IsFeatured).Take(2).ToList()
                : new List<NewsArticle>();

            var totalPages = Math.Max(0, (articles.Count - featuredArticles.Count - 1) / PageSize);
            var actualPageIndex = Math.Max(0, Math.Min(totalPages, pageIndex));

            var pageArticles = articles
                .Where(a => featuredArticles.All(fa => a.Id != fa.Id))
                .Skip(actualPageIndex * PageSize)
                .Take(PageSize)
                .ToList();

            return new IndexModel
            {
                FeaturedArticles = featuredArticles,
                PageArticles = pageArticles,
                PageIndex = actualPageIndex,
                IsFirst = actualPageIndex == 0,
                IsLast = actualPageIndex == totalPages,
                Year = year
            };
        }

        public FullArticleModel BuildFullArticleModel(Guid id)
        {
            var article = newsRepository.GetArticleById(id);
            return article == null ? null : new FullArticleModel { Article = article };
        }
    }
}
