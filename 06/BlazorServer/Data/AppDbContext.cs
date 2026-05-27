using Microsoft.EntityFrameworkCore;
using BlazorServer.Models;

namespace BlazorServer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
}
