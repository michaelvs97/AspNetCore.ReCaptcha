using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.ReCaptcha.NetCore31
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddReCaptcha(Configuration.GetSection("ReCaptcha"));
            services.AddLocalization();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            var supportedLocales =
                "ar,af,am,hy,az,eu,bn,bg,ca,zh-HK,zh-CN,zh-TW,hr,cs,da,nl,en-GB,en,et,fil,fi,fr,fr-CA,gl,ka,de,de-AT,de-CH,el,gu,iw,hi,hu,is,id,it,ja,kn,ko,lo,lv,lt,ms,ml,mr,mn,no,fa,pl,pt,pt-BR,pt-PT,ro,ru,sr,si,sk,sl,es,es-419,sw,sv,ta,te,th,tr,uk,ur,vi,zu"
                    .Split(',')
                    .Select(code => new CultureInfo(code))
                    .ToList();
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                SupportedCultures = supportedLocales,
                SupportedUICultures = supportedLocales,
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Contact}/{action=Index}/{id?}");
            });
        }
    }
}
