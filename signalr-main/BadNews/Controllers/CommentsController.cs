﻿using System;
using System.Linq;
using BadNews.Models.Comments;
using BadNews.Repositories.Comments;
using Microsoft.AspNetCore.Mvc;

namespace BadNews.Controllers
{
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommentsRepository commentsRepository;

        public CommentsController(CommentsRepository commentsRepository)
        {
            this.commentsRepository = commentsRepository;
        }

        // GET
        [HttpGet("api/news/{id}/comments")]
        public ActionResult<CommentsDto> GetCommentsForNews(Guid newsId)
        {
            var dto = new CommentsDto();
            dto.Comments = commentsRepository.GetComments(newsId).Select(x => new CommentDto {User = x.User, Value = x.Value}).ToList();
            dto.NewsId = newsId;
            return new ActionResult<CommentsDto>(dto);
        }
    }
}