using Microsoft.AspNetCore.Http;
using IntuneDriveMapping.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using IntuneDriveMapping.Helpers;
using System.Linq;
using System;

public class DriveMappingStore : IntuneDriveMapping.Helpers.IDriveMappingStore
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string sessionName = "driveMappingList";
    private readonly string errosSession = "lastError";

    const string poshInsertString = "!INTUNEDRIVEMAPPINGJSON!";
    const string poshTemplateName = "IntuneDriveMappingTemplate.ps1";
    const string poshremoveStaleDrives = "$removeStaleDrives = $false";

    public DriveMappingStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<DriveMapping> GetDriveMappings ()
    {
        List<DriveMapping> driveMappings = new List<DriveMapping>();
        string configuration = _httpContextAccessor.HttpContext.Session.GetString(sessionName);
        
        try
        {
            JsonConvert.DeserializeObject<List<DriveMapping>>(configuration).ForEach(
                entry => driveMappings.Add(entry)
            );
        }
        catch
        {
            // nothing we can do
        }

        return driveMappings;
    }


    public string GetPowerShell (bool removeStaleDrives)
    {
        string poshTemplate = System.IO.File.ReadAllText(@"wwwroot/bin/" + poshTemplateName);

        string jsonConfig = JsonConvert.SerializeObject(GetDriveMappings());

        poshTemplate = poshTemplate.Replace(poshInsertString, jsonConfig);

        if (removeStaleDrives)
        {
            poshTemplate = poshTemplate.Replace(poshremoveStaleDrives, poshremoveStaleDrives.Replace("false", "true"));
        }

        return poshTemplate;
    }

    public void SetDriveMappings(List<DriveMapping> driveMappings)
    {
         driveMappings.OrderBy(entry => entry.Id);
        _httpContextAccessor.HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings));
    }

    public void SetErrorMessage (Exception exception)
    {
        _httpContextAccessor.HttpContext.Session.SetString(errosSession, exception.Message);
    }

    public string GetErrorMessage()
    {
        string error = _httpContextAccessor.HttpContext.Session.GetString(errosSession);

        // Clear to only display once
        _httpContextAccessor.HttpContext.Session.Remove(errosSession);

        return error;
    }
}