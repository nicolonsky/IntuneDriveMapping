using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using IntuneDriveMapping.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace IntuneDriveMapping.Helpers
{
    public static class Converter
    {
        /// <summary>
        /// Helper method to convert a group policy preference to a drive mapping model entity
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>List<DriveMappingModel></returns> 
        public static List<DriveMappingModel> ConvertToDriveMappingList (IFormFile xml)
        {
            // create xmldoc
            XmlDocument xmldoc = new XmlDocument();

            xmldoc.Load(xml.OpenReadStream());

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

            return driveMappings;
        }

        /// <summary>
        ///  Fetch the PowerShell config variable which contains a JSON serialized DriveMapping List
        /// </summary>
        /// <param name="powershell"></param>
        /// <param name="poshConfigVariable"></param>
        /// <returns>List<DriveMappingModel></returns>
        public static List<DriveMappingModel> ParsePowerShellConfiguration (IFormFile powershell, string poshConfigVariable)
        {
            string driveMappingConfig = null;
            string line;

            // Read first few lines from PowerShell scripts until json configuration section
            using (StreamReader reader = new StreamReader(powershell.OpenReadStream()))
            {
                line = reader.ReadLine();

                while (!line.Contains(poshConfigVariable) && line != null)
                {
                    line = reader.ReadLine();
                }

                if (line != null && line.StartsWith(poshConfigVariable))
                {
                    driveMappingConfig = line.Replace(poshConfigVariable, "");
                    driveMappingConfig = driveMappingConfig.Trim();
                    driveMappingConfig = driveMappingConfig.TrimStart('\'');
                    driveMappingConfig = driveMappingConfig.TrimEnd('\'');
                }

                return JsonConvert.DeserializeObject<List<DriveMappingModel>>(driveMappingConfig);
            }
        }
    }
}
