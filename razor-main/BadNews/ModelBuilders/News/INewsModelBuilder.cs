using System;
using BadNews.Models.News;
using BadNews.Repositories.News;

namespace BadNews.ModelBuilders.News
{
    public interface INewsModelBuilder
    {
        FullArticleModel BuildFullArticleModel(Guid id);
        IndexModel BuildIndexModel(int pageIndex, bool useFeaturedArticles, int? year);
    }
}