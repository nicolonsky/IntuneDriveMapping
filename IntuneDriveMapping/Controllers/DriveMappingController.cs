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
using System.Text;

namespace IntuneDriveMapping.Controllers
{
    public class DriveMappingController : Controller
    {
        const string sessionName = "driveMappingList";
        const string errosSession = "lastError";
        const string indexView = "Index";

        public ActionResult Index()
        {
            ViewBag.ShowList = false;

            //check if error message is stored in session & forward to view
            if(HttpContext.Session.GetString(errosSession) != null)
            {
                ViewBag.Error = HttpContext.Session.GetString(errosSession);

                //clear session after returned to view
                HttpContext.Session.Remove(errosSession);

            }

            if (HttpContext.Session.GetString(sessionName)==null)
            {
                return View();
            }
            else
            {
                ViewBag.ShowList = true;

                List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                return View(driveMappings);

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

                    XmlNodeList driveProperties = xmldoc.SelectNodes("q1:GPO/q1:User/q1:ExtensionData/q1:Extension/q2:DriveMapSettings/q2:Drive", nsmanager);


                    //create list to store all entries
                    List<DriveMappingModel> driveMappings = new List<DriveMappingModel>();

                    DriveMappingModel driveMapping;

                    //helper index to assign id to our entries 
                    int i = 0;

                    foreach (XmlNode property in driveProperties)
                    {

                        //the real drive mapping configuration is stored in the 2nd XML child-node --> index 1
                        driveMapping = new DriveMappingModel
                        {
                            Path = property.ChildNodes[1].Attributes["path"].InnerXml,
                            DriveLetter = property.ChildNodes[1].Attributes["letter"].InnerXml,
                            Label = property.ChildNodes[1].Attributes["label"].InnerXml,
                            Identifier = (i + 1)
                        };

                        //check if we have a filter applied as child node --> index 2
                        try
                        {

                            string groupFilter= property.ChildNodes[2].ChildNodes[0].Attributes["name"].InnerXml;

                            driveMapping.GroupFilter = groupFilter;
                        }
                        catch
                        {
                            //nothing we can do
                        }


                        driveMappings.Add(driveMapping);

                        i++;
                    }

                    HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings));

                }

                return RedirectToAction(indexView);
            }

            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.ToString());

                return RedirectToAction(indexView);
            }
        }

        public ActionResult Edit(int? id)
        {
            try
            {
                List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                var driveMappingEntry = driveMappings.Where(s => s.Identifier == id).FirstOrDefault();

                //prevent user passing invalid index by url
                if (driveMappingEntry == null) {

                    throw new NullReferenceException();
                }
                else
                {
                    return View(driveMappingEntry);
                }
            }

            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession,ex.ToString());

                return RedirectToAction(indexView);

            }
        }

        [HttpPost]
        public ActionResult Edit(DriveMappingModel driveMapping)
        {

            try
            {

                if (ModelState.IsValid)
                {

                    //haven't found better solution --> improvement needed!
                    //so i just remove the existing entry and add the new one and do a resort of the list

                    List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                    driveMappings.RemoveAt(driveMapping.Identifier - 1);

                    driveMappings.Add(driveMapping);

                    HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings.OrderBy(entry => entry.Identifier)));
                }
                else
                {
                    return View();

                }

                return RedirectToAction(indexView);

            }

            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.ToString());

                return RedirectToAction(indexView);

            }
        }

        public ActionResult Delete(int? id)
        {
            try
            {
                List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                var driveMappingEntry = driveMappings.Where(s => s.Identifier == id).FirstOrDefault();

                return View(driveMappingEntry);

            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.ToString());

                return RedirectToAction(indexView);

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                var driveMappingEntry = driveMappings.Where(s => s.Identifier == id).FirstOrDefault();

                driveMappings.Remove(driveMappingEntry);

                HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings));

                return RedirectToAction(indexView);
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.ToString());

                return RedirectToAction(indexView);

            }
  
        }

        public ActionResult Download()
        {
            try
            {
                //string poshTemplate = PhysicalFile(@"wwwroot/bin/IntuneDriveMappingTemplate.ps1","default/text").ToString();


                // poshTemplate.Replace("!INTUNEDRIVEMAPPINGJSON!", HttpContext.Session.GetString(sessionName));


                // return File(Encoding.UTF8.GetBytes(poshTemplate),"default/text","drivemapping.ps1");

               return RedirectToAction(indexView);
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.ToString());

                return RedirectToAction(indexView);

            }
        }

        public ActionResult ResetSession()
        {
            try
            {
                HttpContext.Session.Clear();

                return RedirectToAction(indexView);

            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.ToString());

                return RedirectToAction(indexView);

            }
        }
    }
}




