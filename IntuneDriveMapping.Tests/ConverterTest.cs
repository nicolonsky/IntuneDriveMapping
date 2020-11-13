using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using IntuneDriveMapping.Models;
using IntuneDriveMapping.Helpers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Moq;
using IntuneDriveMapping.Tests;
using Newtonsoft.Json;
using System.Text;

namespace IntuneDriveMapping.Tests
{
    [TestClass]
    public class ConverterTest
    {
        // Example GPP which contains entries for three drive mappings
        private readonly string gppXml = @"TestFiles/DriveMapping.xml";

        // Example PowerShell script which contains entries for three drive mappings
        private readonly string powerShellScript = @"TestFiles/DriveMapping.ps1";

        private readonly string[] entries = { "\\\\intra.tech.nicolonsky.ch\\Home", "\\\\intra.tech.nicolonsky.ch\\TechBlog", "\\\\intra.tech.nicolonsky.ch\\Projects" };


        /// <summary>
        /// Test conversion from group policy preference XML to a driveMapping list
        /// </summary>
        [TestMethod]
        public void TestGroupPolicyPreferenceXmlConversion()
        {
            FileStream xml = File.Open(gppXml, FileMode.Open);

            List<DriveMapping> driveMappings = Converter.ConvertToDriveMappingList(xml);

            // Check number of entries
            Assert.IsTrue(driveMappings.Count == 3);

            foreach (string entry in entries)
            {
                Assert.IsNotNull(driveMappings.Find(drive => drive.Path.Equals(entry)));
            }
        }

        /// <summary>
        /// Test conversion from existing drive mapping PowerShell to a driveMapping list
        /// </summary>
        [TestMethod]
        public void TestPowerShellConfigurationExtraction()
        {
            FileStream powerShell = File.Open(powerShellScript, FileMode.Open);

            List<DriveMapping> driveMappings = Converter.ParsePowerShellConfiguration(powerShell);

            // Check number of entries
            Assert.IsTrue(driveMappings.Count == 3);

            foreach (string entry in entries)
            {
                Assert.IsNotNull(driveMappings.Find(drive => drive.Path.Equals(entry)));
            }
        }
 
        //[TestMethod]
        //public void TestPowerShellGeneration()
        //{
        //    FileStream xml = File.Open(gppXml, FileMode.Open);

        //    List<DriveMapping> driveMappings = Converter.ConvertToDriveMappingList(xml);


        //    //Mock IHttpContextAccessor
        //    //Mock IHttpContextAccessor
        //    var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        //    var context = new DefaultHttpContext();

        //    //context.Session.Set("driveMappingList", Encoding.ASCII.GetBytes());
        //    mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
        //    mockHttpContextAccessor.Setup(_ => _.HttpContext.Session.GetString("driveMappingLis")).Returns(JsonConvert.SerializeObject(driveMappings));
        //    //Mock HeaderConfiguration

        //    IDriveMappingStore store = new DriveMappingStore(mockHttpContextAccessor.Object);

        //    store.SetDriveMappings(driveMappings);

        //    CollectionAssert.AreEqual(driveMappings, store.GetDriveMappings());
        //}
    }
}
