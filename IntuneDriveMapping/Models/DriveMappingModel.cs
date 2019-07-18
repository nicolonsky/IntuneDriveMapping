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

    public class AadAppRegistration
    {
        [Required]
        [Display(Name = "Tenant ID")]
        [RegularExpression(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$", ErrorMessage = "Specify your tenant ID")]
        public string TenantId { get; set; }
        [Required]
        [Display(Name = "Client ID")]
        [RegularExpression(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$", ErrorMessage = "Specify your client ID")]
        public string ClientId { get; set; }
        [Required]
        [Display(Name = "Client secret")]
        public string ClientSecret { get; set; }

        public string Scope { get; } = "https://graph.microsoft.com/.default";

        public string GrantType { get; } = "client_credentials";
    }
}
