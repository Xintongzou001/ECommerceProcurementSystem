using ECommerceProcurementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProcurementSystem.Data
{
    public class ProcurementContext : DbContext
    {
        public ProcurementContext(DbContextOptions<ProcurementContext> options)
            : base(options)
        {
        }

        // 📌 DbSets for all entities
        public DbSet<City> Cities { get; set; }
        public DbSet<AnnualReport> AnnualReports { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
        public DbSet<Commodity> Commodities { get; set; }
        public DbSet<MasterAgreement> MasterAgreements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Table naming (optional, uncomment if needed)
            // modelBuilder.Entity<City>().ToTable("City");
            // modelBuilder.Entity<AnnualReport>().ToTable("AnnualReport");
            // modelBuilder.Entity<Vendor>().ToTable("Vendor");

            // 🔁 AnnualReport → City (1:Many)
            modelBuilder.Entity<AnnualReport>()
                .HasOne(a => a.City)
                .WithMany(c => c.AnnualReports)
                .HasForeignKey(a => a.CityID);

            // 🔁 AnnualReport → Vendor (1:Many)
            modelBuilder.Entity<AnnualReport>()
                .HasOne(a => a.Vendor)
                .WithMany(v => v.AnnualReports)
                .HasForeignKey(a => a.Vendor_Code);

            // 🗝 PurchaseOrder primary key
            modelBuilder.Entity<PurchaseOrder>()
                .HasKey(p => p.Purchase_Order);

            // 🗝 Composite key for PurchaseOrderLine
            modelBuilder.Entity<PurchaseOrderLine>()
                .HasKey(l => new { l.Purchase_Order, l.CommodityID });

            // PurchaseOrderLine → PurchaseOrder (1:Many)
            modelBuilder.Entity<PurchaseOrderLine>()
                .HasOne<PurchaseOrder>()
                .WithMany(po => po.Lines)
                .HasForeignKey(l => l.Purchase_Order);

            // PurchaseOrderLine → Commodity (1:Many)
            modelBuilder.Entity<PurchaseOrderLine>()
                .HasOne(l => l.Commodity)
                .WithMany()
                .HasForeignKey(l => l.CommodityID);

            // PurchaseOrder → Vendor (1:Many)
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(p => p.Vendor)
                .WithMany(v => v.PurchaseOrders)
                .HasForeignKey(p => p.Vendor_Code);

            // PurchaseOrder → MasterAgreement (1:Many)
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(p => p.Agreement)
                .WithMany()
                .HasForeignKey(p => p.Master_Agreement);
        }
    }
}