namespace Repo;

using Microsoft.EntityFrameworkCore;

public class ApplicationContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Document> Documents => Set<Document>();

    public DbSet<SmevOrder> SmevOrder => Set<SmevOrder>();

    public DbSet<EpguOrder> EpguOrder => Set<EpguOrder>();

    public DbSet<ConfigSettings> ConfigSettings => Set<ConfigSettings>();

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=Repository/Repository.db");
        }
    }
}