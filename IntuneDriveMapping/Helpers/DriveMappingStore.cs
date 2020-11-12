using Microsoft.AspNetCore.Http;
using IntuneDriveMapping.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using IntuneDriveMapping.Helpers;
using System.Linq;

public class DriveMappingStore : IntuneDriveMapping.Helpers.IDriveMappingStore
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string sessionName = "driveMappingList";

    public DriveMappingStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<DriveMappingModel> GetDriveMappings ()
    {
        string configuration = _httpContextAccessor.HttpContext.Session.GetString(sessionName);
        return JsonConvert.DeserializeObject<List<DriveMappingModel>>(configuration);
    }

    public void SetDriveMappings(List<DriveMappingModel> driveMappings)
    {
         driveMappings.OrderBy(entry => entry.Id);
        _httpContextAccessor.HttpContext.Session.SetString(sessionName, JsonConvert.SerializeObject(driveMappings));
    }
}