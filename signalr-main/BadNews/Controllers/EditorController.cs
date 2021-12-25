﻿using System;
using BadNews.Elevation;
using BadNews.Models.Editor;
using BadNews.Repositories.News;
using Microsoft.AspNetCore.Mvc;

namespace BadNews.Controllers
{
    [ElevationRequiredFilter]
    public class EditorController : Controller
    {
        private readonly INewsRepository newsRepository;

        public EditorController(INewsRepository newsRepository)
        {
            this.newsRepository = newsRepository;
        }

        public IActionResult Index()
        {
            return View(new IndexViewModel());
        }

        [HttpPost]
        public IActionResult CreateArticle([FromForm] IndexViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var id = newsRepository.CreateArticle(new NewsArticle
            {
                Date = DateTime.Now.Date,
                Header = model.Header,
                Teaser = model.Teaser,
                ContentHtml = model.ContentHtml,
            });

            return RedirectToAction("FullArticle", "News", new
            {
                id = id
            });
        }

        [HttpPost]
        public IActionResult DeleteArticle(Guid id)
        {
            newsRepository.DeleteArticleById(id);
            return RedirectToAction("Index", "News");
        }
    }
}
