// Этот код будет вызыван для всех тэгов, у которых есть атрибуты data-val=true и data-val-stopwords
// Он нужен, чтобы подключить правила проверки и подготовить для них параметры
$.validator.unobtrusive.adapters.add('stopwords', ['commaSeparatedWords'], function (options) {
    // Если к тэгу добавлен атрибут data-val-stopwords-commaSeparatedWords,
    // то его значение можно получить, передав во втором аргументе его имя без префикса,
    // т.е. передав 'commaSeparatedWords'.
    // Тогда значение будет находится в options.params под ключом commaSeparatedWords.
    var commaSeparatedWords = options.params.commaSeparatedWords;

    // Подготовка параметров для правила проверки stopwords-rule и его подключение
    // NOTE: обычно правила проверки называются без префикса "rule-for-"
    options.rules['rule-for-stopwords'] = {
        stopWords: commaSeparatedWords.split(',')
    };
    // Задание сообщения для проверки
    // Здесь используется то, что передано с сервера, но можно генерировать на лету
    options.messages['rule-for-stopwords'] = (params, element) =>
        "#client " + options.message;
});

// Здесь определяется правило проверки
$.validator.addMethod('rule-for-stopwords', function (value, element, params) {
    // Получение подготовленного параметра
    var stopWords = params.stopWords;
    
    // Код проверки, аналогичный коду серверной проверки на C#
    if (!value)
        return true;
    var cleanText = value.replace(/[^а-яёa-z ]/g, ' ');
    var hasStopWord = stopWords.some(it => cleanText.toLowerCase().indexOf(it.toLowerCase()) >= 0);
    return !hasStopWord;
});
