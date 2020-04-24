using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.ReCaptcha.NetCore21
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddReCaptcha(Configuration.GetSection("ReCaptcha"));
            services.AddLocalization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Contact}/{action=Index}/{id?}");
            });
        }
    }
}
