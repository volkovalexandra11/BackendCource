using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace BadNews.Validation
{
    public class StopWordsAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private readonly IValidationAttributeAdapterProvider baseProvider = new ValidationAttributeAdapterProvider();

        public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer stringLocalizer)
        {
            return attribute is StopWordsAttribute wordsAttribute 
                ? new StopWordsAttributeAdapter(wordsAttribute, stringLocalizer) 
                : baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
        }
    }
}
