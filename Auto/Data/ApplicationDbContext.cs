using Auto.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auto
    .Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Automobil> autos { get; set; }
        public DbSet<Hersteller> herstellers { get; set; }
        public DbSet<Parameter> parameters { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
