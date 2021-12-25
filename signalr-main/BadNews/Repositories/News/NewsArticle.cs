using System;

namespace BadNews.Repositories.News
{
    public class NewsArticle
    {
        public Guid Id { get; set; }
        public bool IsDeleted  { get; set; }
        public bool IsFeatured  { get; set; }
        public string Header { get; set; }
        public DateTime Date { get; set; }
        public string Teaser { get; set; }
        public string ContentId { get; set; }
        public string ContentHtml { get; set; }

        public NewsArticle()
        {
        }

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
