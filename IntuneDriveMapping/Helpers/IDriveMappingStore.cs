using IntuneDriveMapping.Models;
using System;
using System.Collections.Generic;

namespace IntuneDriveMapping.Helpers
{
    public interface IDriveMappingStore
    {
        List<DriveMappingModel> GetDriveMappings();
        void SetDriveMappings(List<DriveMappingModel> driveMappings);
        string GetErrorMessage();
        void SetErrorMessage(Exception exception);
    }
}
