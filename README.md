# AspNetCore.ReCaptcha
[![NuGet Version](http://img.shields.io/nuget/v/AspNetCore.ReCaptcha.svg?style=flat)](https://www.nuget.org/packages/AspNetCore.ReCaptcha/) 
[![Coverage Status](https://coveralls.io/repos/github/michaelvs97/AspNetCore.ReCaptcha/badge.svg?branch=master)](https://coveralls.io/github/michaelvs97/AspNetCore.ReCaptcha?branch=master)

ReCAPTCHA Library for .NET 6 and above.

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
        "Version": "v2", // The ReCaptcha version to use, can be v2, v2invisible or v3
        "UseRecaptchaNet": false, // Value whether to use google recaptcha or recaptcha.net
        "ScoreThreshold": 0.5, // Only applicable for recaptcha v3, specifies the score threshold when it is considered successful
    }
}
```

## Usage
To use AspNetCore.ReCaptcha in your project, you must add the ReCaptcha services to the service container like so:

```C#
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));
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
To be able to make use of the taghelper, you will need to include the following line of code in your `_ViewImports.cshtml`:
```cshtml
@addTagHelper *, AspNetCore.ReCaptcha
```

The action that you will be posting to (in this case SubmitForm) will need the following attribute on the method:

```C#
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

### Manual Validation
If you need to validate a recaptcha but cannot use the `[ValidateReCaptcha]` attribute for some reason (maybe you're using a JSON POST instead of form POST).
In that case you can inject the `AspNetCore.ReCaptcha.IReCaptchaService` service and use one of the two methods on there:
`VerifyAsync` verifies the provided token against the recaptcha service and returns whether or not it is successful. For V3 this also checks if the score is greater than or equal to the ScoreThreshold that can be configured in the appsettings.
`GetVerifyResponseAsync` calls the recaptcha service with the provided token and returns the response. The response will contain whether it is successful or not and for V3 what the score is.

### Language support
By default, AspNetCore.ReCaptcha will use the language that is being used in the request. So we will make use of the Culture of the `HttpContext`. However, you can override this by specifying a language in the ReCaptcha element. This is shown in the next example:
```cshtml
@Html.ReCaptcha(language: "en-GB")
```

```cshtml
<recaptcha language="en-GB" />
```
We support all languages supported by ReCaptcha, list can be found [here](https://developers.google.com/recaptcha/docs/language).

This will only change the language of the recaptcha widget that is shown, not the language of the message that is shown when the recaptcha failed.

To localize the error message (e.g. the message set with `[ValidateReCaptcha(ErrorMessage = "")]`), add the message
to the resource file of the controller or razor page that the validate attribute is used. For an example of this check
the [Razor Pages sample](https://github.com/michaelvs97/AspNetCore.ReCaptcha/tree/master/Samples/AspNetCore.ReCaptcha.RazorPages)
and look at the Pages/ContactModel.cs and Resources/Pages/ContactModel.resx files.

You can learn more about request localization in ASP.NET Core [here](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-8.0)

## Examples
The two sample projects show two different use cases of using the library. One is using the MVC pattern, the other
uses the Razor Pages pattern. The sample with Razor Pages is also configured with request localization.

[MVC](https://github.com/michaelvs97/AspNetCore.ReCaptcha/tree/master/Samples/AspNetCore.ReCaptcha.Mvc)

[Razor Pages](https://github.com/michaelvs97/AspNetCore.ReCaptcha/tree/master/Samples/AspNetCore.ReCaptcha.RazorPages)
