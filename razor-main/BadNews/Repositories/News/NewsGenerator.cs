using System;
using System.Collections.Generic;
using System.IO;

namespace BadNews.Repositories.News
{
    public class NewsGenerator
    {
        private readonly Random random = new Random();
        private readonly string[] subjects = BuildSubjects();
        private readonly string[] actions = BuildActions();
        private readonly string[] objects = BuildObjects();
        private readonly string trashContentHtml = ReadHtmlContent("0");

        public IEnumerable<NewsArticle> GenerateNewsArticles()
        {
            var currentYear = DateTime.Now.Year;
            var sampleNews = BuildSampleNews(currentYear);
            foreach (var it in sampleNews)
                yield return it;
            while (true)
                yield return GenerateTrashNewsArticle(currentYear - 4, currentYear - 2);
        }

        private NewsArticle GenerateTrashNewsArticle(int minYear, int maxYear)
        {
            return new NewsArticle
            {
                Id = Guid.NewGuid(),
                IsDeleted = false,
                Date = GenerateTrashDate(minYear, maxYear),
                ContentId = "0",
                ContentHtml = trashContentHtml,
                Header = GenerateTrashHeader(),
                Teaser = "",
            };
        }

        private DateTime GenerateTrashDate(int minYear, int maxYear)
        {
            var year = random.Next(minYear, maxYear + 1);
            var month = random.Next(1, 12 + 1);
            var day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);
            var date = new DateTime(year, month, day);
            return date;
        }

        private string GenerateTrashHeader()
        {
            var subject = subjects[random.Next(0, subjects.Length)];
            var action = actions[random.Next(0, actions.Length)];
            var @object = objects[random.Next(0, objects.Length)];
            return $"{subject} {action} {@object}";
        }

        private static string[] BuildSubjects() => new string[] {
            "Восьмиклассник",
            "Программист",
            "Математик",
            "Инженер",
            "Студент",
            "Преподаватель",
            "Бобер",
            "Пес",
            "Кот",
            "Кролик",
        };

        private static string[] BuildActions() => new string[] {
            "потряс",
            "побежал",
            "съел",
            "отработал",
            "занес",
            "закодил",
            "оценил",
            "рассчитал",
            "разнес",
            "решил",
        };

        private static string[] BuildObjects() => new string[] {
            "Касперского",
            "быстро",
            "медленно",
            "высоко",
            "низко",
            "круто",
            "пафосно",
            "конкретно",
            "точно",
            "инфекцию",
        };

        private static NewsArticle[] BuildSampleNews(int currentYear)
        {
            var articles = new[] {
                new NewsArticle
                {
                    Id = Guid.Parse("5ab19137-3e28-4eca-bd19-3185ebeba0c6"),
                    Date = new DateTime(currentYear, 1, 1),
                    Header = $"Настал Новый год!",
                    Teaser = "",
                    ContentId = "hny"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("cf7abef6-ccb2-479e-a3c8-4bddd7e188d5"),
                    Date = new DateTime(currentYear - 1, 11, 2),
                    Header = "Грета Тунберг попросила помочь ей попасть на конференцию по климату",
                    Teaser = "Экоактивистка Грета Тунберг может не успеть попасть на конференцию в Испании, так как она сейчас находится в США, а ей нужно в Европу, но самолетами она не пользуется принципиально. Поэтому Грета попросила помощи у своих читателей в Twitter. Они порекомендовали ей плыть на дельфинах или использовать свиток полета из Morrowind.",
                    ContentId = "1"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("7faac679-c3e0-4e18-8cb3-099aab973f6e"),
                    Date = new DateTime(currentYear - 1, 2, 17),
                    Header = "В Гонконге украли туалетную бумагу",
                    Teaser = "В Гонконге вооруженные люди похитили 600 рулонов туалетной бумаги. В городе опасаются ее нехватки из-за коронавируса.",
                    ContentId = "2"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("84302034-4209-4bb3-95c6-33e4c47210bd"),
                    IsFeatured = true,
                    Date = new DateTime(currentYear - 1, 3, 22),
                    Header = "Ограбление Почты России",
                    Teaser = "В Ижевске двое пришли в отделение Почты России с оружием и похитили мешок с деньгами. В мешке находились 8000 десятикопеечных монет на сумму 835 рублей.",
                    ContentId = "3"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("84190720-151b-4614-8547-3ace50ca64af"),
                    IsFeatured = true,
                    Date = new DateTime(currentYear - 1, 3, 22),
                    Header = "Отправленный под домашний арест москвич пропил свой электронный браслет",
                    Teaser = "Потрясающий ход",
                    ContentId = "4"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("45f3ab98-8946-4025-8fe2-848a1b4d5314"),
                    Date = new DateTime(currentYear - 1, 1, 11),
                    Header = "Сибиряк забрал любимый унитаз после развода с женой",
                    Teaser = "Не бросил верного друга!",
                    ContentId = "5"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("ba860141-57ec-46cf-a0dd-1468e5ade6fc"),
                    Date = new DateTime(currentYear - 1, 9, 10),
                    Header = "Кассир из Японии украл деньги с 1300 кредитных карт",
                    Teaser = "Кассир из Японии украл деньги с 1300 банковских карт. Он просто запоминал все цифры, начиная от номера и заканчивая CVV-кодом. Правда попался он, когда заказал в интернете две сумки стоимостью $2500. Его быстро нашла полиция - для доставки он указал домашний адрес.",
                    ContentId = "6"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("8d55cc44-ac39-4909-a458-5e20cbf59fdb"),
                    IsFeatured = true,
                    Date = new DateTime(currentYear - 1, 11, 11),
                    Header = "В Индии мужчина решил попасть в тюрьму, чтобы спастись от голода",
                    Teaser = "В Индии мужчина был арестован за то, что планировал попасть за решетку с целью получить еду и крышу над головой.",
                    ContentId = "7"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("6bac74d3-2d69-41aa-9b62-a878bab3f3f5"),
                    Date = new DateTime(currentYear - 1, 12, 28),
                    Header = "Женщина решила соблазнить мужа чулками со змеиным принтом",
                    Teaser = "Но что-то пошло не так",
                    ContentId = "8"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("4c365418-1823-49e3-a863-079f0a42b014"),
                    Date = new DateTime(currentYear - 1, 08, 15),
                    Header = "Бомжа в Саратовской области отправили под домашний арест",
                    Teaser = "",
                    ContentId = "9"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("253cbe87-7219-4c0a-83e4-621bad445c8a"),
                    Date = new DateTime(currentYear - 1, 10, 25),
                    Header = "Подстреленный олень притворился мертвым и убил охотника в США",
                    Teaser = "",
                    ContentId = "10"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("9ba49f66-769a-4982-9def-8df90b1ac3d3"),
                    Date = new DateTime(currentYear - 1, 10, 5),
                    Header = "Летающие автомобили начнут собирать в Новосибирске",
                    Teaser = "",
                    ContentId = "11"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("a40c5b1a-e5a5-438a-b897-cf4a29298f4e"),
                    Date = new DateTime(currentYear - 1, 08, 30),
                    Header = "В Италии врач за девять лет проработал всего 15 дней",
                    Teaser = "В Италии врач за девять лет службы присутствовал на рабочем месте всего 15 дней. Как пишет The Telegraph, медику удавалось законно уклоняться от работы: все это время он провел на курсах, в отпуске и на больничном. При этом он получал полноценную зарплату.",
                    ContentId = "12"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("c11f0ac0-7f66-4e9c-a53c-23983698b995"),
                    IsFeatured = true,
                    Date = new DateTime(currentYear - 1, 05, 16),
                    Header = "Студент год обманом получал бесплатные обеды в KFC",
                    Teaser = "Студент из Южной Африки арестован за то, что целый год бесплатно питался в KFC. Он просто приходил и говорил, что главный офис прислал его, чтобы проверить, соответствует ли их курочка стандартам качества.",
                    ContentId = "13"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("cfd82b82-5eeb-4c8c-90e4-4768fce2e039"),
                    Date = new DateTime(currentYear - 1, 08, 23),
                    Header = "Блогер заблудился в лесу, снимая видео о том, как не заблудиться в лесу",
                    Teaser = "",
                    ContentId = "14"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("4a858f72-040a-4974-8daf-2c9e64eb682b"),
                    Date = new DateTime(currentYear - 1, 08, 29),
                    Header = "В Исландии женщина попыталась найти саму себя",
                    Teaser = "Женщина случайно присоединилась к поисковой бригаде, которая искала ее саму",
                    ContentId = "15"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("733396f2-d0c1-4314-aa7c-f5e063945910"),
                    Date = new DateTime(currentYear - 1, 6, 16),
                    Header = "Китаец продавал туры в Чернобыль, но возил туристов в Челябинск",
                    Teaser = "",
                    ContentId = "16"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("48ed9c57-bbdb-4f85-9485-fa7e5ebb40bf"),
                    IsFeatured = true,
                    Date = new DateTime(currentYear - 1, 06, 13),
                    Header = "Пенсионеры получили две квартиры вместо одной и расстроились",
                    Teaser = "Пара пожилых москвичей поссорилась и начала готовиться к разводу. По совету юриста заранее разделили квартиру, оказавшись владельцами комнат, а затем по программе реновации получили две квартиры вместо одной и помирились",
                    ContentId = "17"
                },
                new NewsArticle
                {
                    Id = Guid.Parse("d0cba9d4-8889-4626-bbe2-49065cd7d145"),
                    Date = new DateTime(currentYear - 1, 10, 11),
                    Header = "Молодожены перебрали с алкоголем и ненароком купили отель",
                    Teaser = "Молодожены из Лондона так напились в свой медовый месяц на Шри-Ланке, что случайно купили отель вместо того, чтобы снять комнату",
                    ContentId = "18"
                }
            };

            foreach (var it in articles)
                if (it.ContentHtml == null && it.ContentId != null)
                    it.ContentHtml = ReadHtmlContent(it.ContentId);

            return articles;
        }

        private static string ReadHtmlContent(string contentId)
        {
            if (!string.IsNullOrEmpty(contentId))
            {
                var cd = Directory.GetCurrentDirectory();
                var path = $"./$Content/NewsArticles/{contentId}.html";
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path);
                    return text;
                }
                else
                {
                    throw new FileNotFoundException($"Can't find file for contendId={contentId}");
                }
            }
            return "";
        }
    }
}
