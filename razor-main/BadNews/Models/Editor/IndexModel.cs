using System.ComponentModel.DataAnnotations;
using BadNews.Validation;

namespace BadNews.Models.Editor
{
    public class IndexViewModel
    {
        [Required(ErrorMessage = "У новости должен быть заголовок")]
        public string Header { get; set; }

        [StopWords("действительно", "реально", "на самом деле", "поверьте", "без обмана",
            ErrorMessage = "Нельзя использовать стоп-слова")]
        public string Teaser { get; set; }
        
        [StopWords("действительно", "реально", "на самом деле", "поверьте", "без обмана",
            ErrorMessage = "Нельзя использовать стоп-слова")]
        public string ContentHtml { get; set; }
    }
}
