using Microsoft.EntityFrameworkCore;
using InventoryManagement.Models;

public class ApplicationDbContext : DbContext
{
    // constructor to pass options to the base DbContext
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        
    }

    // create tables for InventoryItem and Order
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}