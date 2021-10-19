using BadNews.ModelBuilders.News;
using BadNews.Models.News;
using BadNews.Repositories.News;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Serilog;

namespace BadNews
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration configuration;

        // В конструкторе уже доступна информация об окружении и конфигурация
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            this.configuration = configuration;
        }

        // В этом методе добавляются сервисы в DI-контейнер
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<INewsRepository, NewsRepository>();
            services.AddSingleton<INewsModelBuilder, NewsModelBuilder>();
            var mvcBuilder = services.AddControllersWithViews();
            if (env.IsDevelopment())
                mvcBuilder.AddRazorRuntimeCompilation();
        }

        // В этом методе конфигурируется последовательность обработки HTTP-запроса
        public void Configure(IApplicationBuilder app)
        {
            // app.UseDeveloperExceptionPage();
            // app.UseExceptionHandler("/Errors/Exception");
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Errors/Exception");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSerilogRequestLogging();
            app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("status-code", "StatusCode/{code?}", new
                {
                    controller = "Errors",
                    action = "StatusCode"
                });
                endpoints.MapControllerRoute("default", "{controller=News}/{action=Index}");
            });
            app.Map("/news/fullarticle", fullArticleApp =>
            {
                fullArticleApp.Run(RenderFullArticlePage);
            });

            // Остальные запросы — 404 Not Found
        }

        // Региональные настройки, которые используются при обработке запросов новостей.
        private static CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-ru");

        private async Task RenderFullArticlePage(HttpContext context)
        {
            // Model Builder достается из DI-контейнера
            var newsModelBuilder = context.RequestServices.GetRequiredService<INewsModelBuilder>();

            // Извлекаются входные параметры запроса
            var idString = context.Request.Path.Value.Split('/').ElementAtOrDefault(1);
            Guid.TryParse(idString, out var id);

            // Строится модель страницы
            var model = newsModelBuilder.BuildFullArticleModel(id);

            // Строится HTML для модели
            string pageHtml = BuildFullArticlePageHtml(model);

            // Результат записывается в ответ
            await context.Response.WriteAsync(pageHtml);
        }

        private static string BuildFullArticlePageHtml(FullArticleModel model)
        {
            var pageTemplate = File.ReadAllText("./$Content/Templates/FullArticle.hbs");
            var pageHtml = pageTemplate
                .Replace("{{header}}", model.Article.Header)
                .Replace("{{date}}", model.Article.Date.ToString("d MMM yyyy", culture))
                .Replace("{{content}}", model.Article.ContentHtml);
            return pageHtml;
        }
    }
}
