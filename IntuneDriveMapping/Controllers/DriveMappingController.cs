using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using IntuneDriveMapping.Models;
using System.Web;
using Newtonsoft.Json;

namespace IntuneDriveMapping.Controllers
{
    public class DriveMappingController : Controller
    {

        public ActionResult Index()
        {
            if (TempData["userData"] == null)
            {
                ViewBag.ShowList = false;
                return View();
            }
            else
            {

                List<DriveMappingModel> lst = JsonConvert.DeserializeObject<List<DriveMappingModel>>(TempData["userData"].ToString());

                ViewBag.ShowList = true;
                return View(lst);
            }
        }
        [HttpPost]
        public ActionResult Upload()
        {
            try
            {
                List<DriveMappingModel> driveMappings = new List<DriveMappingModel>();
                var file = Request.Form.Files[0];
                if (file != null && file.Length > 0)
                {
                    XmlDocument xmldoc = new XmlDocument();

                    xmldoc.Load(file.OpenReadStream());

                    XmlNamespaceManager nsmanager = new XmlNamespaceManager(xmldoc.NameTable);

                    nsmanager.AddNamespace("q1", xmldoc.DocumentElement.NamespaceURI);
                    nsmanager.AddNamespace("q2", "http://www.microsoft.com/GroupPolicy/Settings/DriveMaps");

                    DriveMappingModel driveMapping;
                    XmlNodeList usernodes = xmldoc.SelectNodes("q1:GPO/q1:User/q1:ExtensionData/q1:Extension/q2:DriveMapSettings/q2:Drive/q2:Properties", nsmanager);
                    foreach (XmlNode usr in usernodes)
                    {
                        #pragma warning disable IDE0017 // Simplify object initialization
                        driveMapping = new DriveMappingModel();
                        #pragma warning restore IDE0017 // Simplify object initialization

                        driveMapping.Id = 1;
                        driveMapping.Path = usr.Attributes["path"].InnerXml;
                        driveMapping.DriveLetter = usr.Attributes["letter"].InnerXml;
                        driveMapping.Label = usr.Attributes["label"].InnerXml;

                        driveMappings.Add(driveMapping);
                    }


                    TempData["userData"] = JsonConvert.SerializeObject(driveMappings);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var error = ex;
                throw;
            }
        }
    }
}




