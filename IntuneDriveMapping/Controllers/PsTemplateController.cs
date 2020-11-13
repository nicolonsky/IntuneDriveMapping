using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IntuneDriveMapping.Models;
using Newtonsoft.Json;

namespace IntuneDriveMapping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PsTemplateController : ControllerBase
    {
        const string poshTemplateName = "IntuneDriveMappingTemplate.ps1";
        const string contentRoot = "wwwroot/bin/";
        const string poshInsertString = "!INTUNEDRIVEMAPPINGJSON!";

        [HttpGet]
        public string GetPs()
        {
            string poshTemplate = System.IO.File.ReadAllText( @contentRoot + poshTemplateName);
            return poshTemplate;
        }

        [HttpPost]
        public string GeneratePs (IEnumerable<DriveMapping> driveMappings)
        {
            string poshTemplate = System.IO.File.ReadAllText(@contentRoot + poshTemplateName);
            poshTemplate =  poshTemplate.Replace(poshInsertString, JsonConvert.SerializeObject(driveMappings));
            return poshTemplate;
        }
    }
}
