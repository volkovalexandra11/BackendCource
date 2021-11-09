using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace BadNews.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class StopWordsAttribute : ValidationAttribute
    {
        public string[] StopWords { get; }
        private readonly Regex cleaningRegex = new Regex("[^а-яёa-z ]");

        public StopWordsAttribute(params string[] stopWords)
        {
            this.StopWords = stopWords;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var text = value as string;
            if (string.IsNullOrEmpty(text))
                return ValidationResult.Success;

            var cleanText = cleaningRegex.Replace(text, " ");
            
            var hasStopWord = StopWords.Any(it => 
                cleanText.Contains(it, StringComparison.InvariantCultureIgnoreCase));
            return hasStopWord
                ? new ValidationResult(FormatErrorMessage(validationContext.DisplayName))
                : ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            var message = base.FormatErrorMessage(name);
            return $"#server {message}";
        }
    }
}
