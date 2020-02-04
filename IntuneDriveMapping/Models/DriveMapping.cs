using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace IntuneDriveMapping.Models
{
    public class DriveMapping
    {
        public string Path { get; set; }
        public string DriveLetter { get; set; }
        public string Label { get; set; }
        public int Id { get; set; }
        public string GroupFilter { get; set; }

        public IHttpContextAccessor accessor { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;

        public DriveMapping(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<DriveMapping> GetDriveMappings(string HttpSessionName)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return JsonConvert.DeserializeObject<List<DriveMapping>>(httpContext.Session.GetString(HttpSessionName));
        }

        public void SetDriveMappings(IEnumerable<DriveMapping> DriveMappings, string HttpSessionName)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            httpContext.Session.SetString(HttpSessionName, JsonConvert.SerializeObject(DriveMappings.OrderBy(entry => entry.Id)));
        }
    }
}
