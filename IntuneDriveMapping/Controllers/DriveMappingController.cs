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

            ViewBag.ShowList = true;

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

                    XmlNodeList driveProperties = xmldoc.SelectNodes("q1:GPO/q1:User/q1:ExtensionData/q1:Extension/q2:DriveMapSettings/q2:Drive/q2:Properties", nsmanager);

                    XmlNodeList driveFilters= xmldoc.SelectNodes("q1:GPO/q1:User/q1:ExtensionData/q1:Extension/q2:DriveMapSettings/q2:Drive/q2:Filters/q2:FilterGroup", nsmanager);

                    XmlNodeList drivePrimaryKey = xmldoc.SelectNodes("q1:GPO/q1:User/q1:ExtensionData/q1:Extension/q2:DriveMapSettings/q2:Drive", nsmanager);

                    int i = 0;

                    foreach (XmlNode property in driveProperties)
                    {

                        driveMapping = new DriveMappingModel
                        {
                            Path = property.Attributes["path"].InnerXml,
                            DriveLetter = property.Attributes["letter"].InnerXml,
                            Label = property.Attributes["label"].InnerXml,
                            UID = drivePrimaryKey[i].Attributes["uid"].InnerText
                        };

                        /** if (driveFilters[i] != null && driveFilters[i].Attributes["identifier"].InnerText== drivePrimaryKey[i].Attributes["uid"].InnerText)
                        {
                            driveMapping.GroupFilter = driveFilters[i].Attributes["name"].InnerXml;
                        } **/

                        driveMappings.Add(driveMapping);

                        i++;
                    }

                    TempData["driveMappingData"] = JsonConvert.SerializeObject(driveMappings);
                }

                return RedirectToAction("Index");
            }
            catch (XmlException ex)
            {
                ViewBag.Error = "XML parsing error occured. Make sure you uploaded a valid GPP XML! " + ex;
                return View("Index");

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex;
                return View("Index");


            }

        }

        public ActionResult Edit(string UID, string Path, string DriveLetter, string Label)
        {
            //Get the student from studentList sample collection for demo purpose.
            //Get the student from the database in the real application
            //var std = studentList.Where(s => s.StudentId == Id).FirstOrDefault();

            string identifier = UID;
            string targetpath = Path;
            string letter = DriveLetter;
            string dexfription = Label;

            DriveMappingModel driveMapping;

            driveMapping = new DriveMappingModel
            {
                Path = targetpath,
                DriveLetter=letter,
                Label=dexfription,
                UID = identifier
            };
            
            return View("Edit");
        }
    }
}




