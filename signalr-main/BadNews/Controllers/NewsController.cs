using BadNews.ModelBuilders.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace BadNews.Controllers
{
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client, VaryByHeader = "Cookie")]
    public class NewsController : Controller
    {
        private readonly INewsModelBuilder newsModelBuilder;
        private readonly IMemoryCache memoryCache;

        public NewsController(INewsModelBuilder newsModelBuilder, IMemoryCache memoryCache)
        {
            this.newsModelBuilder = newsModelBuilder;
            this.memoryCache = memoryCache;
        }

        public IActionResult Index(int? year, int pageIndex = 0)
        {
            var model = newsModelBuilder.BuildIndexModel(pageIndex, !year.HasValue, year);
            return View(model);
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult FullArticle(Guid id)
        {
            //string cacheKey = $"{nameof(NewsController)}_{nameof(FullArticle)}_{id}";
            //if (!memoryCache.TryGetValue(cacheKey, out var model))
            //{
            //    model = newsModelBuilder.BuildFullArticleModel(id);
            //    if (model != null)
            //    {
            //        memoryCache.Set(cacheKey, model, new MemoryCacheEntryOptions
            //        {
            //            SlidingExpiration = TimeSpan.FromSeconds(30)
            //        });
            //    }
            //}

            var model = newsModelBuilder.BuildFullArticleModel(id);
            if (model == null)
                return NotFound();
            return View(model);
        }
    }
}