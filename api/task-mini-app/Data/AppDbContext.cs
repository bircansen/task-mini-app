using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaskApi.Models;

namespace TaskApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskLog> TaskLogs => Set<TaskLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasColumnName("full_name").IsRequired();
            e.Property(x => x.Email).HasColumnName("email").IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.ToTable("tasks");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasColumnName("title").IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Status).HasColumnName("status").IsRequired();
            e.Property(x => x.AssigneeUserId).HasColumnName("assignee_user_id");
            e.Property(x => x.DueDate).HasColumnName("due_date");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasOne(x => x.AssigneeUser)
             .WithMany(u => u.Tasks)
             .HasForeignKey(x => x.AssigneeUserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TaskLog>(e =>
        {
            e.ToTable("task_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.TaskId).HasColumnName("task_id").IsRequired();
            e.Property(x => x.Action).HasColumnName("action").IsRequired();
            e.Property(x => x.OldValue).HasColumnName("old_value");
            e.Property(x => x.NewValue).HasColumnName("new_value");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");

            e.HasOne(x => x.Task)
             .WithMany(t => t.Logs)
             .HasForeignKey(x => x.TaskId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}