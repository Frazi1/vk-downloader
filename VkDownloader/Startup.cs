using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Vkontakte;
using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.SessionStorage;
using Blazored.Toast;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VkDownloader.Http;
using VkDownloader.Vk;
using VkDownloader.Vk.Wall;

namespace VkDownloader
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .WriteTo.LogzIo(Configuration.GetValue<string>("LogzioToken"),"dotnet", true, dataCenterSubDomain:"listener-nl").MinimumLevel.Warning()
                .WriteTo.Console();

            services.AddLogging(builder => { builder.AddSerilog(loggerConfiguration.CreateLogger()); });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = $"/Authentication/signin?provider={VkontakteAuthenticationDefaults.AuthenticationScheme}";
                    options.Events.OnSigningOut = (CookieSigningOutContext context) =>
                    {
                        context.Response.Redirect("/");
                        return Task.CompletedTask;
                    };
                })
                .AddVkontakte(options =>
                {
                    options.Scope.Add("groups");
                    options.ApiVersion = "5.126";
                    options.ClientId = Configuration.GetValue<string>("VkAppId");
                    options.ClientSecret = Configuration.GetValue<string>("VkClientSecret");
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.SaveTokens = true;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;
                    // options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                    // options.CorrelationCookie.IsEssential = true;
                    options.Events.OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/");
                        context.HandleResponse();
                        
                        return Task.FromResult(0);
                    };
                });
            
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddBlazoredSessionStorage();
            services.AddBlazoredLocalStorage();
            services.AddBlazoredToast();
            services.AddBlazoredModal();
            services.AddBlazorise()
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();
            
            services.AddControllers();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ISession>(provider => provider.GetRequiredService<IHttpContextAccessor>().HttpContext?.Session);
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddHttpClient("vk").AddTraceLogHandler(_ => true);

            // services.AddScoped<VkAuthLogic>();
            services.AddScoped<VkImagesService>();
            services.AddScoped<WallStateStorage>();
            services.AddScoped<VkImageDownloader>()
                .Configure<VkImageDownloaderSettings>(Configuration.GetSection("VkDownloader"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.ApplicationServices
                .UseBootstrapProviders()
                .UseFontAwesomeIcons();
            
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });
            
        }
    }
}