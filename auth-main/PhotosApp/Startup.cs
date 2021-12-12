using System;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotosApp.Clients;
using PhotosApp.Clients.Models;
using PhotosApp.Data;
using PhotosApp.Models;
using PhotosApp.Services.Authorization;
using Serilog;

namespace PhotosApp
{
    public class Startup
    {
        private IWebHostEnvironment Env { get; }
        private IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.Env = env;
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<PhotosServiceOptions>(Configuration.GetSection("PhotosService"));

            var mvc = services.AddControllersWithViews();
            if (Env.IsDevelopment())
                mvc.AddRazorRuntimeCompilation();

            // NOTE: Подключение IHttpContextAccessor, чтобы можно было получать HttpContext там,
            // где это не получается сделать более явно.
            services.AddHttpContextAccessor();

            var connectionString = Configuration.GetConnectionString("PhotosDbContextConnection")
                ?? "Data Source=PhotosApp.db";
            services.AddDbContext<PhotosDbContext>(o => o.UseSqlite(connectionString));
            // NOTE: Вместо Sqlite можно использовать LocalDB от Microsoft или другой SQL Server
            //services.AddDbContext<PhotosDbContext>(o =>
            //    o.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=PhotosApp;Trusted_Connection=True;"));

            services.AddScoped<IPhotosRepository, LocalPhotosRepository>();

            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<PhotoEntity, PhotoDto>().ReverseMap();
                cfg.CreateMap<PhotoEntity, Photo>().ReverseMap();

                cfg.CreateMap<EditPhotoModel, PhotoEntity>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());
            }, Array.Empty<Assembly>());

            services.AddTransient<ICookieManager, ChunkingCookieManager>();
            services.AddAuthentication()
                .AddOpenIdConnect("Passport", "Паспорт", options =>
                {
                    options.Authority = "https://localhost:7001";

                    options.ClientId = "Photos App by OIDC";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    // NOTE: oidc и profile уже добавлены по-умолчанию
                    options.Scope.Add("email");

                    options.CallbackPath = "/signin-passport";
                    options.SignedOutCallbackPath = "/signout-callback-passport";

                    // NOTE: все эти проверки токена выполняются по умолчанию, указаны для ознакомления
                    options.TokenValidationParameters.ValidateIssuer = true; // проверка издателя
                    options.TokenValidationParameters.ValidateAudience = true; // проверка получателя
                    options.TokenValidationParameters.ValidateLifetime = true; // проверка не протух ли
                    options.TokenValidationParameters.RequireSignedTokens = true; // есть ли валидная подпись издателя
                    options.SaveTokens = true;
                });

            services.AddScoped<IAuthorizationHandler, MustOwnPhotoHandler>();
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.AddPolicy(
                    "Beta",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.RequireClaim("testing", "beta");
                    });
                options.AddPolicy(
                    "CanAddPhoto",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.RequireClaim("subscription", "paid");
                    });
                options.AddPolicy(
                    "MustOwnPhoto",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.AddRequirements(new MustOwnPhotoRequirement());
                    });
                options.AddPolicy(
                    "Dev",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                    });
            });
            services.AddAuthentication(options =>
            {
                // NOTE: Схема, которую внешние провайдеры будут использовать для сохранения данных о пользователе
                // NOTE: Так как значение совпадает с DefaultScheme, то эту настройку можно не задавать
                options.DefaultSignInScheme = "Cookie";
                // NOTE: Схема, которая будет вызываться, если у пользователя нет доступа
                options.DefaultChallengeScheme = "Passport";
                // NOTE: Схема на все остальные случаи жизни
                options.DefaultScheme = "Cookie";
            })
            .AddCookie("Cookie", options =>
            {
                // NOTE: Пусть у куки будет имя, которое расшифровывается на странице «Decode»
                options.Cookie.Name = "PhotosApp.Auth";
                // NOTE: Если не задать здесь путь до обработчика logout, то в этом обработчике
                // будет игнорироваться редирект по настройке AuthenticationProperties.RedirectUri
                options.LogoutPath = "/Passport/Logout";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Exception");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default", "{controller=Photos}/{action=Index}/{id?}");
            });
        }
    }
}
