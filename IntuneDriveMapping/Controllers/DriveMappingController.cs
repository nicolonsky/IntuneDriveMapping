using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using IntuneDriveMapping.Models;
using Newtonsoft.Json;

namespace IntuneDriveMapping.Controllers
{
    public class DriveMappingController : Controller
    {

        
        public ActionResult Index()
        {
            if (TempData["driveMappingData"] == null)
            {
                ViewBag.ShowList = false;
                return View();
            }
            else
            {

                List<DriveMappingModel> lst = JsonConvert.DeserializeObject<List<DriveMappingModel>>(TempData["driveMappingData"].ToString());

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

                    nsmanager.AddNamespace("q1", "http://www.microsoft.com/GroupPolicy/Settings");
                    nsmanager.AddNamespace("q2", "http://www.microsoft.com/GroupPolicy/Settings/DriveMaps");

                    DriveMappingModel driveMapping;

                    XmlNodeList usernodes = xmldoc.SelectNodes("q1:GPO/q1:User/q1:ExtensionData/q1:Extension/q2:DriveMapSettings/q2:Drive/q2:Properties", nsmanager);

                    foreach (XmlNode usr in usernodes)
                    {
                        driveMapping = new DriveMappingModel();

                        driveMapping.Path = usr.Attributes["path"].InnerXml;
                        driveMapping.DriveLetter = usr.Attributes["letter"].InnerXml;
                        driveMapping.Label = usr.Attributes["label"].InnerXml;

                        driveMappings.Add(driveMapping);
                    }

                    TempData["driveMappingData"] = JsonConvert.SerializeObject(driveMappings);
                }

                return RedirectToAction("Index");
            }
            catch (XmlException ex)
            {
                ViewBag.Error = "XML parsing error occured. Make sure you uploaded a valid GPP XML! " + ex.Message;
                return View("Index");

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Index");


            }
        }
    }
}




