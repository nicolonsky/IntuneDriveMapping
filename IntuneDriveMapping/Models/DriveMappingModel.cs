using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IntuneDriveMapping.Models
{
    public class DriveMappingModel
    {
        [Display(Name ="UNC Path")]
        [Required]
        [RegularExpression(@"^\\\\[a-zA-Z0-9\.\-_]{1,}(\\[a-zA-Z0-9-%\-_]{1,}){1,}[\$]{0,1}", ErrorMessage = "Valid UNC path required")]
        public string Path { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z]{1}$", ErrorMessage = "Specify single character")]
        [Display(Name = "Drive Letter")]
        public string DriveLetter { get; set; }
        [Display(Name = "Display Name")]
        public string Label { get; set; }
        [Key]
        public int Id { get; set; }
        [Display(Name = "Security Group Filter")]
        public string GroupFilter { get; set; }
    }
}
