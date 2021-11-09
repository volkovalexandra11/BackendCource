using System;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace BadNews.Validation
{
    public class StopWordsAttributeAdapter : AttributeAdapterBase<StopWordsAttribute>
    {
        public StopWordsAttributeAdapter(StopWordsAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer) { }

        public override void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Включаем unobtrusive validation для этого тэга
            MergeAttribute(context.Attributes, "data-val", "true");
            // Добавляем сообщение об ошибке, которое будет использовано валидатором на JS для ошибок Stop Words
            MergeAttribute(context.Attributes, "data-val-stopwords", GetErrorMessage(context));

            // Добавляем список стоп-слов из атрибута модели в атрибут тэга,
            // чтобы валидатор на JS этот список достал и использовал
            MergeAttribute(context.Attributes, "data-val-stopwords-commaSeparatedWords", string.Join(",", Attribute.StopWords));
        }

        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            // Формируем сообщение об ошибке для клиента
            // Клиент может использовать его, либо генерировать самостоятельно
            var message = GetErrorMessage(
                validationContext.ModelMetadata,
                validationContext.ModelMetadata.GetDisplayName());
            return $"#adapter {message}";
        }
    }
}
