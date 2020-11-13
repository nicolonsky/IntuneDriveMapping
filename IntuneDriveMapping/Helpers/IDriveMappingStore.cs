using IntuneDriveMapping.Models;
using System;
using System.Collections.Generic;

namespace IntuneDriveMapping.Helpers
{
    public interface IDriveMappingStore
    {
        List<DriveMapping> GetDriveMappings();
        void SetDriveMappings(List<DriveMapping> driveMappings);
        string GetErrorMessage();
        void SetErrorMessage(Exception exception);
        string GetPowerShell(bool removeStaleDrives);
    }
}
