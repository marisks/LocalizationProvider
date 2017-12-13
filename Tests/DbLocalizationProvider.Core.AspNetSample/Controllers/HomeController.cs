﻿using System;
using System.Diagnostics;
using DbLocalizationProvider.AspNetCore;
using DbLocalizationProvider.Core.AspNetSample.Models;
using DbLocalizationProvider.Core.AspNetSample.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DbLocalizationProvider.Core.AspNetSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IStringLocalizer<SampleResources> _localizer2;

        public HomeController(IStringLocalizer<HomeController> localizer, IStringLocalizer<SampleResources> localizer2)
        {
            _localizer = localizer;
            _localizer2 = localizer2;
        }

        public IActionResult Index()
        {
            ViewData["TestString"] = _localizer.GetString(() => SampleResources.PageHeader);
            ViewData["TestString2"] = _localizer2.GetString(r => r.PageHeader2);

            return View(new UserViewModel());
        }

        [HttpPost]
        public IActionResult Index(UserViewModel model)
        {
            return View(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
