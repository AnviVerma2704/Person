﻿using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
