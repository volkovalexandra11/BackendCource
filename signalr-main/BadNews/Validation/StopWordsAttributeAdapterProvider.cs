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
            if (attribute is StopWordsAttribute)
                return new StopWordsAttributeAdapter(attribute as StopWordsAttribute, stringLocalizer);
                
            return baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
        }
    }
}
