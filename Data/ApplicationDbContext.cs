using Microsoft.EntityFrameworkCore;
using TaskFlowAISrv.Models;

namespace TaskFlowAISrv.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<TaskSubItem> TaskSubItems { get; set; }
}
