using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using IntuneDriveMapping.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using IntuneDriveMapping.Helpers;

namespace IntuneDriveMapping.Controllers
{
    public class DriveMappingController : Controller
    {   
        //Configuration params for the generated PowerShell script
        const string poshInsertString = "!INTUNEDRIVEMAPPINGJSON!";
        const string poshTemplateName = "IntuneDriveMappingTemplate.ps1";
        const string poshExportName = "DriveMapping.ps1";
        const string poshConfigVariable = "$driveMappingJson =";
        const string poshremoveStaleDrives = "$removeStaleDrives = $false";

        //default view where everything comes together
        const string indexView = "Index";

        private readonly IDriveMappingStore _driveMappingStore;

        public DriveMappingController(IDriveMappingStore dependency)
        {
            _driveMappingStore = dependency;
        }


        public ActionResult Index()
        {
            //don't display any table data if no content is available
            ViewBag.ShowList = false;

            //check if error message is stored in session & forward to view
            string error = _driveMappingStore.GetErrorMessage();
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Error = error;
            }

            //check if a drivemapping list exists and display it
            if (!_driveMappingStore.GetDriveMappings().Any())
            {
                return View();
            }
            else 
            {
                List<DriveMappingModel> driveMappings = _driveMappingStore.GetDriveMappings();
                ViewBag.ShowList = true;

                var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                if (isAjax)
                {
                    return PartialView("_Table", driveMappings);
                }

                return View(driveMappings);
            }
        }

        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        public ActionResult Init()
        {
            List<DriveMappingModel> driveMappings = new List<DriveMappingModel>();
            DriveMappingModel driveMappingModel = new DriveMappingModel
            {
                DriveLetter = "A",
                Label = "Example",
                Path = "\\\\path\\to\\your\\share",
                GroupFilter = "exampleGroupSamAccountName"
            };

            driveMappings.Add(driveMappingModel);
            _driveMappingStore.SetDriveMappings(driveMappings);
            return RedirectToAction(indexView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DriveMappingModel driveMapping)
        {
            if (ModelState.IsValid)
            {
                List<DriveMappingModel> driveMappings = _driveMappingStore.GetDriveMappings();
                driveMapping.Id = driveMappings.Count + 1;
                driveMappings.Add(driveMapping);
                _driveMappingStore.SetDriveMappings(driveMappings);
            }

            return PartialView("_Create", driveMapping);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                List<DriveMappingModel> driveMappings = new List<DriveMappingModel>();

                if (file.FileName.EndsWith(".ps1"))
                {
                    // Retrieve JSON configuration from PowerShell file
                    driveMappings = Converter.ParsePowerShellConfiguration(file, poshConfigVariable);    
                }
                else if (file.FileName.EndsWith(".xml"))
                {
                    // Convert XML to drive mapping entity
                     driveMappings = Converter.ConvertToDriveMappingList(file);
                }
                else
                {
                    throw new InvalidDataException("Invalid file uploaded!");
                }

                _driveMappingStore.SetDriveMappings(driveMappings);
                return RedirectToAction(indexView);
            }
            catch (Exception ex)
            {
                _driveMappingStore.SetErrorMessage(ex);
                return RedirectToAction(indexView);
            }
        }

        public ActionResult Edit(int? Id)
        {
            try
            {
                List<DriveMappingModel> driveMappings = _driveMappingStore.GetDriveMappings();
                var driveMappingEntry = driveMappings.Where(s => s.Id == Id).FirstOrDefault();

                //prevent user passing invalid index by url
                if (driveMappingEntry == null) {

                    throw new NullReferenceException();
                }
                else
                {
                    return PartialView("_Edit", driveMappingEntry);
                }
            }

            catch (Exception ex)
            {
                _driveMappingStore.SetErrorMessage(ex);
                return RedirectToAction(indexView);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DriveMappingModel driveMapping)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<DriveMappingModel> driveMappings = _driveMappingStore.GetDriveMappings();
                    DriveMappingModel selectedItem = driveMappings.Where(dm => dm.Id == driveMapping.Id).First();
                    driveMappings[driveMappings.IndexOf(selectedItem)] = driveMapping;
                    _driveMappingStore.SetDriveMappings(driveMappings);
                }

                 return PartialView("_Edit", driveMapping);
            }

            catch (Exception ex)
            {
                _driveMappingStore.SetErrorMessage(ex);
                return RedirectToAction(indexView);
            }
        }

        [HttpGet]
        public ActionResult Delete(int? Id)
        {
            try
            {
                List<DriveMappingModel> driveMappings = _driveMappingStore.GetDriveMappings();

                var driveMappingEntry = driveMappings.Where(s => s.Id == Id).FirstOrDefault();

                if (driveMappingEntry == null)
                {
                    throw new NullReferenceException();
                }
                else
                {
                    return PartialView("_Delete", driveMappingEntry);
                }
            }
            catch (Exception ex)
            {
                _driveMappingStore.SetErrorMessage(ex);
                return RedirectToAction(indexView);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int Id)
        {
            try
            {
                List<DriveMappingModel> driveMappings = _driveMappingStore.GetDriveMappings();

                var driveMappingEntry = driveMappings.Where(s => s.Id == Id).FirstOrDefault();

                driveMappings.Remove(driveMappingEntry);

                _driveMappingStore.SetDriveMappings(driveMappings);

                return RedirectToAction(indexView);
            }
            catch (Exception ex)
            {
                _driveMappingStore.SetErrorMessage(ex);
                return RedirectToAction(indexView);
            }
        }

        [HttpPost]
        public ActionResult Download(bool removeStaleDrives)
        {
            try
            {
                if (_driveMappingStore.GetDriveMappings().Any())
                {
                    //load the PowerShell template and replace values with generated configuration

                    string poshTemplate = System.IO.File.ReadAllText(@"wwwroot/bin/" + poshTemplateName);

                    string jsonConfig = JsonConvert.SerializeObject(_driveMappingStore.GetDriveMappings());

                    poshTemplate = poshTemplate.Replace(poshInsertString, jsonConfig);

                    if (removeStaleDrives)
                    {
                        poshTemplate = poshTemplate.Replace(poshremoveStaleDrives, poshremoveStaleDrives.Replace("false","true"));
                    }

                    //return file download
                    return File(Encoding.UTF8.GetBytes(poshTemplate), "default/text", poshExportName);

                }else
                {
                    throw new Exception("No session data found, session might have expired");
                }
            }
            catch (Exception ex)
            {
                _driveMappingStore.SetErrorMessage(ex);
                return RedirectToAction(indexView);
            }
        }

        public ActionResult Reset()
        {
            return PartialView("_Reset");
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
                _driveMappingStore.SetErrorMessage(ex);
                return RedirectToAction(indexView);
            }
        }
    }
}
