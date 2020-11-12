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

    public DriveMappingStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<DriveMappingModel> GetDriveMappings ()
    {
        List<DriveMappingModel> driveMappings = new List<DriveMappingModel>();
        string configuration = _httpContextAccessor.HttpContext.Session.GetString(sessionName);
        
        try
        {
            JsonConvert.DeserializeObject<List<DriveMappingModel>>(configuration).ForEach(
                entry => driveMappings.Add(entry)
            );
        }
        catch
        {
            // nothing we can do
        }

        return driveMappings;
    }

    public void SetDriveMappings(List<DriveMappingModel> driveMappings)
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