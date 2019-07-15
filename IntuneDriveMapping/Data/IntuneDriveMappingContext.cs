using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IntuneDriveMapping.Models
{
    public class IntuneDriveMappingContext : DbContext
    {
        public IntuneDriveMappingContext (DbContextOptions<IntuneDriveMappingContext> options)
            : base(options)
        {
        }

        public DbSet<IntuneDriveMapping.Models.DriveMappingModel> DriveMappingModel { get; set; }
    }
}
