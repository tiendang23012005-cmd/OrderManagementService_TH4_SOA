
using Microsoft.EntityFrameworkCore;
using OrderManagementService.Models;

namespace OrderManagementService.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(
        DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Bảng orders
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.CustomerName)
                .HasColumnName("customer_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.CustomerEmail)
                .HasColumnName("customer_email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(10, 2);

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Bảng order_items
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.OrderId)
                .HasColumnName("order_id");

            entity.Property(x => x.ProductId)
                .HasColumnName("product_id");

            entity.Property(x => x.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(255);

            entity.Property(x => x.Quantity)
                .HasColumnName("quantity");

            entity.Property(x => x.UnitPrice)
                .HasColumnName("unit_price")
                .HasPrecision(10, 2);

            entity.Property(x => x.TotalPrice)
                .HasColumnName("total_price")
                .HasPrecision(10, 2);
        });
    }
}
