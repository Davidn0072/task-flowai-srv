using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TaskFlowAISrv.Models;

namespace TaskFlowAISrv.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<TaskSubItem> TaskSubItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureImmutableCreatedAt<User>(modelBuilder);
        ConfigureImmutableCreatedAt<TaskItem>(modelBuilder);
        ConfigureImmutableCreatedAt<TaskSubItem>(modelBuilder);
    }

    private static void ConfigureImmutableCreatedAt<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>()
            .Property<DateTime>("CreatedAt")
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
