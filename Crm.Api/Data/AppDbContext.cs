using Crm.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .HasIndex(customer => customer.Email)
            .IsUnique();
    }
}