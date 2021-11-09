using System.ComponentModel.DataAnnotations;
using BadNews.Validation;

namespace BadNews.Models.Editor
{
    public class IndexViewModel
    {
        private const string TitleRequiredMessage = "У новости должен быть заголовок";
        private const string StopWordsProhibited = "Нельзя использовать стоп-слова";
        
        [Required(ErrorMessage = TitleRequiredMessage)]
        public string Header { get; init; }

        [StopWords("действительно", "реально", "на самом деле", "поверьте", "без обмана",
            ErrorMessage = StopWordsProhibited)]
        public string Teaser { get; init; }
        
        [StopWords("действительно", "реально", "на самом деле", "поверьте", "без обмана",
            ErrorMessage = StopWordsProhibited)]
        public string ContentHtml { get; init; }
    }
}
