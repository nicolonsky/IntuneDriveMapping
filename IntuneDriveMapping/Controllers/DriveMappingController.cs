using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using IntuneDriveMapping.Models;
using System.Web;

namespace IntuneDriveMapping.Controllers
{
    public class DriveMappingController : Controller
    {
        // GET: Users  
        public ActionResult Index()
        {
            if (TempData["userData"] == null)
            {
                ViewBag.ShowList = false;
                return View();
            }
            else
            {
                List<DriveMappingModel> lst = (List<DriveMappingModel>)TempData["userData"];
                ViewBag.ShowList = true;
                return View(lst);
            }
        }
        [HttpPost]
        public ActionResult Upload()
        {
            try
            {
                List<DriveMappingModel> userList = new List<DriveMappingModel>();
                var file = Request.Form.Files[0];
                if (file != null && file.Length > 0)
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(file.OpenReadStream());
                    DriveMappingModel user;
                    XmlNodeList usernodes = xmldoc.SelectNodes("gpo/User/ExtensionData/Extension/DriveMapSettings/Drive/Properties");
                    foreach (XmlNode usr in usernodes)
                    {
                        user = new DriveMappingModel
                        {
                            Id = Convert.ToInt32(usr["id"].InnerText),
                            Path = usr["path"].InnerText,
                            DriveLetter = usr["letter"].InnerText,
                            Label = usr["label"].InnerText
                        };

                        userList.Add(user);
                    }
                    TempData["userData"] = userList;
                }
                return RedirectToAction("DriveMapping");
            }
            catch (Exception ex)
            {
                var error = ex;
                throw;
            }
        }
    }
}




