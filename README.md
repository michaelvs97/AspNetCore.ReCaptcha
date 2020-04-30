
# AspNetCore.ReCaptcha
ReCAPTCHA Library for .NET Core 2.x/3.x/5.x.

## Requirements
This package requires a secret key as well as a site key provided by ReCaptcha. You can aquire your keyset at [https://www.google.com/recaptcha/intro/v3.html](https://www.google.com/recaptcha/intro/v3.html). It's possible to use either v2 or v3 ReCaptcha.

## Installation
You can install this package using NuGet. You can use the following command:

```shell
# Package Manager
PM> Install-Package AspNetCore.ReCaptcha

# .NET CLI
dotnet add package AspNetCore.ReCaptcha
```

## Configuration
Place the aquired secret key and site key in the appsettings.json of your project. An example of the appsettings file is below:

```json
{
    "ReCaptcha": {
        "SiteKey": "your site key here",
        "SecretKey": "your secret key here",
        "Version": "v2"
    }
}
```

## Usage
To use AspNetCore.ReCaptcha in your project, you must add the following code to your startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services) {
    services.AddReCaptcha(Configuration.GetSection("ReCaptcha"));
}
```

In your .cshtml file you add the following using statement:

```cshtml
@using AspNetCore.ReCaptcha
```

And then you can add the ReCaptcha element to your DOM using the following code or make use of the tag-helper:

```cshtml
@Html.ReCaptcha()
```
```cshtml
<recaptcha />
```

The action that you will be posting to (in this case SubmitForm) will need the following attribute on the method:

```csharp
[ValidateReCaptcha]
[HttpPost]
public IActionResult SubmitForm(ContactViewModel model)
{
    if (!ModelState.IsValid)
        return View("Index");

    TempData["Message"] = "Your form has been sent!";
    return RedirectToAction("Index");
}
```

### Language support
By default, AspNetCore.ReCaptcha will use the language that is being used in the request. So we will make use of the Culture of the `HttpContext`. However, you can overrule this by specifying a language in the ReCaptcha element. This is shown in the next example:
```cshtml
@Html.ReCaptcha(language: "en-GB")
```

```cshtml
<recaptcha language="en-GB" />
```
We support all languages supported by ReCaptcha, list can be found [here](https://developers.google.com/recaptcha/docs/language).

You can learn more about request localization in .NET Core [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-3.1)

## Examples
For every version of .NET Core there is a configured example included in this repository. Check the Samples map for those examples.
