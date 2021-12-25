using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BadNews.Repositories.News
{
    public class NewsIndexedRepository : INewsRepository
    {
        private static readonly string recordSeparator = "<--e891b395-4498-4f93-84a5-19b867d826ae-->";
        private readonly string dataFileName;
        private string DataFilePath => $"./.db/{dataFileName}";

        private object dataFileLocker = new object();

        // Индекс по id
        private Dictionary<string, long> idToPosition = null;

        public NewsIndexedRepository(string dataFileName = "news.txt")
        {
            this.dataFileName = dataFileName;
            // Перед началом работы нужно построить индексы
            BuildIndices();
        }

        // Построение индексов
        private void BuildIndices()
        {
            // Построение индекса по id
            var idToPosition = new Dictionary<string, long>();

            ReadFromFile((meta, data, position) =>
            {
                var id = meta.ToString();
                idToPosition[id] = position;
                return false;
            });

            this.idToPosition = idToPosition;
        }

        public void InitializeDataBase(IEnumerable<NewsArticle> articles)
        {
            var dataFileInfo = new FileInfo(DataFilePath);
            if (dataFileInfo.Exists)
                dataFileInfo.Delete();
            if (!dataFileInfo.Directory.Exists)
                dataFileInfo.Directory.Create();

            lock (dataFileLocker)
            {
                var file = dataFileInfo.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
                using (var fileWriter = new StreamWriter(file))
                {
                    foreach (var article in articles)
                    {
                        var storedArticle = new NewsArticle(article);
                        if (storedArticle.Id == Guid.Empty)
                            storedArticle.Id = Guid.NewGuid();
                        AppendArticle(fileWriter, storedArticle);
                    }
                }
            }
        }

        // Новая версия метода с использованием индекса по id
        public NewsArticle GetArticleById(Guid id)
        {
            var idString = id.ToString();
            if (!idToPosition.TryGetValue(idString, out var position))
                return null;

            var (meta, data) =  ReadFromPosition(position);

            var article = JsonConvert.DeserializeObject<NewsArticle>(data.ToString());
            if (id != article.Id)
                throw new InvalidDataException();

            return article.IsDeleted ? null : article;
        }

        // Старая версия метода без использования индекса по id
        public NewsArticle GetArticleByIdOld(Guid id)
        {
            NewsArticle article = null;

            var idString = id.ToString();
            ReadFromFile((meta, data, position) =>
            {
                if (idString == meta.ToString())
                {
                    var obj = JsonConvert.DeserializeObject<NewsArticle>(data.ToString());
                    if (id != obj.Id)
                        throw new InvalidDataException();
                    article = obj;
                }
                return false;
            });

            return article != null && !article.IsDeleted ? article : null;
        }

        public IList<NewsArticle> GetArticles(Func<NewsArticle, bool> predicate = null)
        {
            var articles = new Dictionary<Guid, NewsArticle>();

            ReadFromFile((meta, data, position) =>
            {
                var id = Guid.Parse(meta.ToString());
                var obj = JsonConvert.DeserializeObject<NewsArticle>(data.ToString());
                if (id != obj.Id)
                    throw new InvalidDataException();
                if (obj.IsDeleted || predicate == null || predicate(obj))
                    articles[id] = obj;
                return false;
            });

            return articles
                .Where(it => !it.Value.IsDeleted)
                .Select(it => it.Value)
                .OrderByDescending(it => it.Date)
                .ToList();
        }

        public IList<int> GetYearsWithArticles()
        {
            var years = new Dictionary<Guid, DateTime>();

            ReadFromFile((meta, data, position) =>
            {
                var obj = JsonConvert.DeserializeObject<NewsArticle>(data.ToString());
                if (!obj.IsDeleted)
                    years[obj.Id] = obj.Date;
                else
                    years.Remove(obj.Id);
                return false;
            });

            return years.Select(it => it.Value.Year)
                .Distinct()
                .OrderByDescending(it => it)
                .ToArray();
        }

        public Guid CreateArticle(NewsArticle article)
        {
            if (article.Id != Guid.Empty)
                throw new InvalidOperationException("Creating article should not have id");

            lock (dataFileLocker)
            {
                var file = new FileStream(DataFilePath, FileMode.Append, FileAccess.Write, FileShare.None);
                using (var fileWriter = new StreamWriter(file))
                {
                    var storedArticle = new NewsArticle(article)
                    {
                        Id = Guid.NewGuid()
                    };
                    AppendArticle(fileWriter, storedArticle);

                    return storedArticle.Id;
                }
            }
        }

        public void DeleteArticleById(Guid id)
        {
            lock (dataFileLocker)
            {
                var file = new FileStream(DataFilePath, FileMode.Append, FileAccess.Write, FileShare.None);
                using (var fileWriter = new StreamWriter(file))
                {
                    var storedArticle = new NewsArticle()
                    {
                        Id = id,
                        IsDeleted = true
                    };

                    AppendArticle(fileWriter, storedArticle);
                }
            }
        }

        private void AppendArticle(StreamWriter file, NewsArticle article)
        {
            var meta = article.Id.ToString();
            var data = JsonConvert.SerializeObject(article, Formatting.Indented);

            file.WriteLine(recordSeparator);
            // Чтобы позиция была верной, сбрасываем все в stream, а затем узнаем позицию объекта
            file.Flush();
            var position = file.BaseStream.Position;
            file.WriteLine(meta);
            file.WriteLine(data);

            // При добавлении новой записи нужно обновлять индексы
            UpdateIndices(article, position);
        }

        private void UpdateIndices(NewsArticle article, long position)
        {
            // Обновляем индекс по id
            idToPosition[article.Id.ToString()] = position;
        }

        private void ReadFromFile(
            Func<StringBuilder, StringBuilder, long, bool> onObjectRead)
        {
            if (!File.Exists(DataFilePath))
                return;

            lock (dataFileLocker)
            {
                var file = new FileStream(DataFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (var fileReader = new SeekableStreamTextReader(file, Encoding.UTF8))
                {
                    int objectLine = 0;
                    var metaBuilder = new StringBuilder();
                    var dataBuilder = new StringBuilder();
                    // Задание позиции, соответствующей началу линии
                    long lineStartPosition = fileReader.UsedBytes;
                    // Позиция, с которой начинается объект
                    long objectStartPosition = -1;

                    string line = fileReader.ReadLine();
                    while (line != null)
                    {
                        if (line != recordSeparator)
                        {
                            if (objectLine++ > 0)
                            {
                                dataBuilder.Append(line);
                            }
                            else
                            {
                                metaBuilder.Append(line);
                                // Сохраняем позицию для чтения объекта
                                objectStartPosition = lineStartPosition;
                            }
                        }
                        else
                        {
                            if (metaBuilder.Length > 0 || dataBuilder.Length > 0)
                            {
                                // Передаем не только данные, но и позицию для чтения объекта
                                if (onObjectRead(metaBuilder, dataBuilder, objectStartPosition))
                                    return;
                            }

                            objectLine = 0;
                            metaBuilder = new StringBuilder();
                            dataBuilder = new StringBuilder();
                        }

                        // Обновление позиции, соответствующей началу линии
                        lineStartPosition = fileReader.UsedBytes;

                        line = fileReader.ReadLine();
                    }

                    if (dataBuilder.Length > 0)
                    {
                        if (onObjectRead(metaBuilder, dataBuilder, objectStartPosition))
                            return;
                    }
                }
            }
        }

        // Метод чтения по позиции позволяет использовать индексы
        private (StringBuilder meta, StringBuilder data) ReadFromPosition(long position)
        {
            if (!File.Exists(DataFilePath))
                return (null, null);

            lock (dataFileLocker)
            {
                var file = new FileStream(DataFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (var fileReader = new SeekableStreamTextReader(file, Encoding.UTF8))
                {
                    fileReader.Seek(position, SeekOrigin.Begin);

                    int objectLine = 0;
                    var metaBuilder = new StringBuilder();
                    var dataBuilder = new StringBuilder();
                    
                    string line = fileReader.ReadLine();
                    while (line != null)
                    {
                        if (line != recordSeparator)
                        {
                            if (objectLine++ > 0)
                                dataBuilder.Append(line);
                            else
                                metaBuilder.Append(line);
                        }
                        else
                        {
                            return (metaBuilder, dataBuilder);
                        }
                        line = fileReader.ReadLine();
                    }

                    return (metaBuilder, dataBuilder);
                }
            }
        }
    }
}
