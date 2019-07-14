using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using IntuneDriveMapping.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace IntuneDriveMapping.Controllers
{
    public class DriveMappingController : Controller
    {
        const string sessionName = "driveMappingList";

        public ActionResult Index()
        {
            ViewBag.ShowList = false;

            if (HttpContext.Session.GetString(sessionName)==null)
            {
                ViewBag.Error = "Empty session" + sessionName;
                return View();
            }
            else
            {
                ViewBag.ShowList = true;

                List<DriveMappingModel> lst = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                return View(lst);

            }
        }

        [HttpPost]
        public ActionResult Upload()
        {

            try
            {
                var file = Request.Form.Files[0];

                if (file != null && file.Length > 0)
                {


                    // create xmldoc
                    XmlDocument xmldoc = new XmlDocument();

                    xmldoc.Load(file.OpenReadStream());

                    //namespace manager & URI's needed in order to read GPP nodes
                    XmlNamespaceManager nsmanager = new XmlNamespaceManager(xmldoc.NameTable);

                    nsmanager.AddNamespace("q1", "http://www.microsoft.com/GroupPolicy/Settings");
                    nsmanager.AddNamespace("q2", "http://www.microsoft.com/GroupPolicy/Settings/DriveMaps");

                    XmlNodeList driveProperties = xmldoc.SelectNodes("q1:GPO/q1:User/q1:ExtensionData/q1:Extension/q2:DriveMapSettings/q2:Drive/q2:Properties", nsmanager);

                    XmlNodeList driveFilters = xmldoc.SelectNodes("q1:GPO/q1:User/q1:ExtensionData/q1:Extension/q2:DriveMapSettings/q2:Drive/q2:Filters/q2:FilterGroup", nsmanager);

                    XmlNodeList drivePrimaryKey = xmldoc.SelectNodes("q1:GPO/q1:User/q1:ExtensionData/q1:Extension/q2:DriveMapSettings/q2:Drive", nsmanager);


                    List<DriveMappingModel> driveMappings = new List<DriveMappingModel>();

                    DriveMappingModel driveMapping;


                    int i = 0;

                    foreach (XmlNode property in driveProperties)
                    {

                        driveMapping = new DriveMappingModel
                        {
                            Path = property.Attributes["path"].InnerXml,
                            DriveLetter = property.Attributes["letter"].InnerXml,
                            Label = property.Attributes["label"].InnerXml,
                            id = (i + 1)
                        };

                        driveMappings.Add(driveMapping);

                        i++;
                    }

                    HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings));

                }
                return RedirectToAction("Index");
            }

            catch (XmlException ex)
            {
                ViewBag.Error = "XML parsing error occured. Make sure you uploaded a valid GPP XML! " + ex;
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex;
                return RedirectToAction("Index");
            }
        }

        public ActionResult Edit(int? id)
        {
            
            try
            {
                List<DriveMappingModel> lst = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                var driveMappingEntry = lst.Where(s => s.id == id).FirstOrDefault();

                return View(driveMappingEntry);

            }

            catch (Exception ex)
            {
                ViewBag.Error = ex;

                return RedirectToAction("Index");

            }
        }

        [HttpPost]
        public ActionResult Edit(DriveMappingModel driveMapping)
        {

            try
            {
                List<DriveMappingModel> lst = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));


                lst[lst.FindIndex(ind => ind.Equals(driveMapping.id))] = driveMapping;

                return RedirectToAction("Index");

            }

            catch (Exception ex)
            {
                ViewBag.Error = ex;

                return RedirectToAction("Index");

            }
        }

        public ActionResult Delete(int? id)
        {
            List<DriveMappingModel> lst = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

            var driveMappingEntry = lst.Where(s => s.id == id).FirstOrDefault();

            return View(driveMappingEntry);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            List<DriveMappingModel> lst = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

            var driveMappingEntry = lst.Where(s => s.id == id).FirstOrDefault();

            lst.Remove(driveMappingEntry);

            return RedirectToAction("Index");
        }
    }
}




