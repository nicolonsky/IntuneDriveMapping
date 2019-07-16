using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IntuneDriveMapping.Models;

namespace IntuneDriveMapping.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            try
            {
                 Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                 DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
                 string displayableVersion = $"{@version} ({@buildDate})";

                 ViewBag.Version = displayableVersion;
            }
            catch
            {
                //SunFunNothingTodo
            }
           
            return View();
        }

        public IActionResult DriveMapping()
        {
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
