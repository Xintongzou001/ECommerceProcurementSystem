using System.Reflection.Emit;
using ECommerceProcurementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProcurementSystem.Data
{
    public class ProcurementContext : DbContext
    {
        public ProcurementContext(DbContextOptions<ProcurementContext> options) : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<AnnualSaleAmount> AnnualSaleAmounts { get; set; }
        public DbSet<Vendor> Vendors { get; set; }

        /* protected override void OnModelCreating(ModelBuilder modelBuilder)
         {
             modelBuilder.Entity<City>().ToTable("City");
             modelBuilder.Entity<AnnualSaleAmount>().ToTable("AnnualSaleAmount");
             modelBuilder.Entity<Vendor>().ToTable("Vendor");
         }*/

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AnnualSaleAmount>()
                .HasOne(a => a.City)
                .WithMany(c => c.AnnualSaleAmounts)
                .HasForeignKey(a => a.CityID);

            modelBuilder.Entity<AnnualSaleAmount>()
                .HasOne(a => a.Vendor)
                .WithMany(v => v.AnnualSaleAmounts)
                .HasForeignKey(a => a.Vendor_Code);
        }
    }
}