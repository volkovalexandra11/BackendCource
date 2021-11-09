using System;

namespace BadNews.Repositories.News
{
    public class NewsArticle
    {
        public Guid Id { get; set; }
        public bool IsDeleted  { get; init; }
        public bool IsFeatured  { get; init; }
        public string Header { get; init; }
        public DateTime Date { get; init; }
        public string Teaser { get; init; }
        public string ContentId { get; init; }
        public string ContentHtml { get; set; }

        public NewsArticle() { }

        public NewsArticle(NewsArticle that)
        {
            Id = that.Id;
            IsDeleted = that.IsDeleted;
            IsFeatured = that.IsFeatured;
            Header = that.Header;
            Date = that.Date;
            Teaser = that.Teaser;
            ContentId = that.ContentId;
            ContentHtml = that.ContentHtml;
        }
    }
}
