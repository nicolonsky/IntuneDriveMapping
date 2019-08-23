using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using IntuneDriveMapping.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace IntuneDriveMapping.Controllers
{
    public class DriveMappingController : Controller
    {
        //Http session configs
        const string sessionName = "driveMappingList";
        const string errosSession = "lastError";
        const string aadAppRegSession = "appReg";

        //Configuration params for the generated PowerShell script
        const string poshInsertString = "!INTUNEDRIVEMAPPINGJSON!";
        const string poshTemplateName = "IntuneDriveMappingTemplate.ps1";
        const string poshExportName = "DriveMapping.ps1";
        const string poshConfigVariable = "$driveMappingJson=";
        const string poshConfigVariableEnd = "$driveMappingConfig";

        //default view where everything comes together
        const string indexView = "Index";


        public ActionResult Index()
        {
            //don't display any table data if no content is available
            ViewBag.ShowList = false;

            
            //get version
            try
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
                //string displayableVersion = $"{version} ({buildDate})";

                string displayableVersion = $"{version}";

                ViewBag.Version = displayableVersion;
            }
            catch
            {
                //SunFunNothingTodo
            }


            //check if error message is stored in session & forward to view
            if (HttpContext.Session.GetString(errosSession) != null)
            {
                ViewBag.Error = HttpContext.Session.GetString(errosSession);

                //clear session after returned to view
                HttpContext.Session.Remove(errosSession);

            }


            //check if a drivemapping list exists and display it
            if (HttpContext.Session.GetString(sessionName)==null)
            {
                return View();
            }

            else 
            {
                
                List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                ViewBag.ShowList = true;

                return View(driveMappings);

            }

        }

         public ActionResult Create()
        {

            return View();

        }

        [HttpPost]
        public ActionResult Create(DriveMappingModel driveMapping)
        {

            try
            {

                if (ModelState.IsValid)
                {

                    //check if first ever item is addedd or list with entries already exists
                    if (HttpContext.Session.GetString(sessionName) != null)
                    {
                        List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                        driveMapping.Id = driveMappings.Last().Id + 1;

                        driveMappings.Add(driveMapping);

                        HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings.OrderBy(entry => entry.Id)));
                    }
                    else
                    {
                        List<DriveMappingModel> driveMappings = new List<DriveMappingModel>();

                        driveMappings.Add(driveMapping);

                        HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings.OrderBy(entry => entry.Id)));
                    }
                }
                else
                {
                    return View();

                }

                return RedirectToAction(indexView);

            }

            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.Message.ToString());

                return RedirectToAction(indexView);

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
                    if (file.FileName.Contains(".ps1"))
                    {

                        string powerShellContent;

                        string driveMappingJson;

                        int driveMappingJsonLocation;

                        int driveMappingJsonLocationEnd;

                        StreamReader inputStreamReader = new StreamReader(file.OpenReadStream());

                        powerShellContent = inputStreamReader.ReadToEnd();

                        driveMappingJsonLocation = powerShellContent.IndexOf(poshConfigVariable);

                        driveMappingJsonLocationEnd = powerShellContent.IndexOf(poshConfigVariableEnd);

                        driveMappingJson = powerShellContent.Substring(driveMappingJsonLocation, driveMappingJsonLocationEnd);

                        throw new Exception(driveMappingJson);

                        List <DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(driveMappingJson);

                        HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings));

                        return RedirectToAction(indexView);

                    }
                    else if (file.FileName.Contains(".xml"))
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
                                Id = (i + 1)
                            };

                            //check if we have a filter applied as child node --> index 2
                            try
                            {
                                string groupFilter = property.ChildNodes[2].ChildNodes[0].Attributes["name"].InnerXml;

                                String[] streamlinedGroupFilter = groupFilter.Split('\\');

                                driveMapping.GroupFilter = streamlinedGroupFilter[1];
                            }
                            catch
                            {
                                //nothing we can do
                            }

                            driveMappings.Add(driveMapping);

                            i++;
                        }

                        HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings));
                    }else
                    {
                        throw new NullReferenceException();
                    }
                }
                return RedirectToAction(indexView);
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.Message.ToString());

                return RedirectToAction(indexView);
            }
        }

        public ActionResult Edit(int? Id)
        {
            try
            {
                List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                var driveMappingEntry = driveMappings.Where(s => s.Id == Id).FirstOrDefault();

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
                HttpContext.Session.SetString(errosSession,ex.Message.ToString());

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

                    driveMappings.RemoveAt(driveMapping.Id - 1);

                    driveMappings.Add(driveMapping);

                    HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings.OrderBy(entry => entry.Id)));
                }
                else
                {
                    return View();

                }
                return RedirectToAction(indexView);
            }

            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.Message.ToString());

                return RedirectToAction(indexView);
            }
        }

        public ActionResult Delete(int? Id)
        {
            try
            {
                List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                var driveMappingEntry = driveMappings.Where(s => s.Id == Id).FirstOrDefault();

                if (driveMappingEntry == null)
                {

                    throw new NullReferenceException();
                }
                else
                {
                    return View(driveMappingEntry);
                }
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.Message.ToString());

                return RedirectToAction(indexView);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int Id)
        {
            try
            {
                List<DriveMappingModel> driveMappings = JsonConvert.DeserializeObject<List<DriveMappingModel>>(HttpContext.Session.GetString(sessionName));

                var driveMappingEntry = driveMappings.Where(s => s.Id == Id).FirstOrDefault();

                driveMappings.Remove(driveMappingEntry);

                HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings));

                return RedirectToAction(indexView);
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.Message.ToString());

                return RedirectToAction(indexView);
            }
        }

        public ActionResult Download()
        {
            try
            {
                if (HttpContext.Session.GetString(sessionName)!=null)
                {

                    //load the PowerShell template and replace values with generated configuration

                    string poshTemplate = System.IO.File.ReadAllText(@"wwwroot/bin/" + poshTemplateName);

                    poshTemplate = poshTemplate.Replace(poshInsertString, HttpContext.Session.GetString(sessionName));

                    //return file download
                    return File(Encoding.UTF8.GetBytes(poshTemplate), "default/text", poshExportName);

                }else
                {
                    throw new Exception("No session data found, session might have expired");
                }
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString(errosSession, ex.Message.ToString());

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
                HttpContext.Session.SetString(errosSession, ex.Message.ToString());

                return RedirectToAction(indexView);
            }
        }
    }
}