using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IntuneDriveMapping.Models
{
    public class DriveMappingModel
    {
        [Required]
        [RegularExpression(@"^\\\\[a-zA-Z0-9\.\-_]{1,}(\\[a-zA-Z0-9\-_]{1,}){1,}[\$]{0,1}", ErrorMessage = "Valid UNC path required")]
        public string Path { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z]{1}$", ErrorMessage = "Specify single character")]
        public string DriveLetter { get; set; }
        public string Label { get; set; }
        public int id { get; set; }
        public string GroupFilter { get; set; }
    }
}
