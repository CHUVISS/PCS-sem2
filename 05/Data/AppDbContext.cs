using Microsoft.EntityFrameworkCore;
using ProductionManagement.Models;

namespace ProductionManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductMaterial>()
            .HasKey(pm => new { pm.ProductId, pm.MaterialId });

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(pm => pm.Product)
            .WithMany(p => p.ProductMaterials)
            .HasForeignKey(pm => pm.ProductId);

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(pm => pm.Material)
            .WithMany(m => m.ProductMaterials)
            .HasForeignKey(pm => pm.MaterialId);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(w => w.ProductionLine)
            .WithMany(l => l.WorkOrders)
            .HasForeignKey(w => w.ProductionLineId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed data
        modelBuilder.Entity<Material>().HasData(
            new Material { Id = 1, Name = "Сталь листовая", Quantity = 500, UnitOfMeasure = "кг", MinimalStock = 100 },
            new Material { Id = 2, Name = "Болты М8", Quantity = 1200, UnitOfMeasure = "шт", MinimalStock = 500 },
            new Material { Id = 3, Name = "Краска синяя", Quantity = 30, UnitOfMeasure = "литр", MinimalStock = 50 },
            new Material { Id = 4, Name = "Пластик ABS", Quantity = 80, UnitOfMeasure = "кг", MinimalStock = 100 }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Корпус машины А1", Category = "Корпуса", MinimalStock = 10, ProductionTimePerUnit = 120, Description = "Стальной корпус серии A" },
            new Product { Id = 2, Name = "Панель управления", Category = "Электроника", MinimalStock = 5, ProductionTimePerUnit = 60, Description = "Пластиковая панель" }
        );

        modelBuilder.Entity<ProductMaterial>().HasData(
            new ProductMaterial { ProductId = 1, MaterialId = 1, QuantityNeeded = 15 },
            new ProductMaterial { ProductId = 1, MaterialId = 2, QuantityNeeded = 24 },
            new ProductMaterial { ProductId = 2, MaterialId = 4, QuantityNeeded = 2 }
        );

        modelBuilder.Entity<ProductionLine>().HasData(
            new ProductionLine { Id = 1, Name = "Линия А", Status = "Active", EfficiencyFactor = 1.0f },
            new ProductionLine { Id = 2, Name = "Линия Б", Status = "Stopped", EfficiencyFactor = 1.2f },
            new ProductionLine { Id = 3, Name = "Линия В", Status = "Active", EfficiencyFactor = 0.8f }
        );

        modelBuilder.Entity<WorkOrder>().HasData(
            new WorkOrder
            {
                Id = 1, ProductId = 1, ProductionLineId = 1, Quantity = 5,
                StartDate = DateTime.Today,
                EstimatedEndDate = DateTime.Today.AddDays(3),
                Status = "InProgress", ProgressPercent = 40
            },
            new WorkOrder
            {
                Id = 2, ProductId = 2, ProductionLineId = null, Quantity = 10,
                StartDate = DateTime.Today.AddDays(1),
                EstimatedEndDate = DateTime.Today.AddDays(2),
                Status = "Pending", ProgressPercent = 0
            }
        );
    }
}
