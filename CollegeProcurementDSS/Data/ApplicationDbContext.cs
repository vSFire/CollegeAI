using CollegeProcurementDSS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CollegeProcurementDSS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<PricePrediction> PricePredictions { get; set; }
        public DbSet<ProcurementHistory> ProcurementHistories { get; set; }
    }
}
