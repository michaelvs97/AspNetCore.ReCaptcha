using System.Globalization;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

// Register ReCaptcha
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptchaInvisible")); // Change this to "ReCaptchaInvisible" or "ReCaptchaV3" for a different version

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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
    DefaultRequestCulture = new RequestCulture("nl"),
    SupportedCultures = supportedLocales,
    SupportedUICultures = supportedLocales,
});

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
