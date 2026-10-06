using Microsoft.EntityFrameworkCore;
using RealEstateMarketplace.Models;

namespace RealEstateMarketplace.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }

        public DbSet<User> Users { get; set; }
    }
}
