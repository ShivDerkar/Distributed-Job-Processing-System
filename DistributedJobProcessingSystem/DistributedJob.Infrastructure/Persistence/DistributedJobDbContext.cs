using DistributedJob.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DistributedJob.Infrastructure.Persistence;

public class DistributedJobDbContext : DbContext
{
    public DistributedJobDbContext(DbContextOptions<DistributedJobDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<BackgroundJob> Jobs => Set<BackgroundJob>();
    public DbSet<JobLog> JobLogs => Set<JobLog>();
    public DbSet<WorkerNode> WorkerNodes => Set<WorkerNode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureJobs(modelBuilder);
        ConfigureJobLogs(modelBuilder);
        ConfigureWorkerNodes(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.PasswordHash)
                .IsRequired();
        });
    }

    private static void ConfigureJobs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BackgroundJob>(entity =>
        {
            entity.ToTable("jobs");

            entity.HasKey(job => job.Id);

            entity.Property(job => job.Type)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(job => job.Status)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(job => job.InputPayload)
                .IsRequired();

            entity.Property(job => job.Result);

            entity.Property(job => job.ErrorMessage);

            entity.HasIndex(job => job.Status);

            entity.HasIndex(job => job.Type);

            entity.HasOne(job => job.User)
                .WithMany(user => user.Jobs)
                .HasForeignKey(job => job.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureJobLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobLog>(entity =>
        {
            entity.ToTable("job_logs");

            entity.HasKey(log => log.Id);

            entity.Property(log => log.Message)
                .IsRequired();

            entity.Property(log => log.Level)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(log => log.JobId);

            entity.HasOne(log => log.Job)
                .WithMany(job => job.Logs)
                .HasForeignKey(log => log.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureWorkerNodes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkerNode>(entity =>
        {
            entity.ToTable("worker_nodes");

            entity.HasKey(worker => worker.Id);

            entity.Property(worker => worker.WorkerName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(worker => worker.Status)
                .HasConversion<string>()
                .IsRequired();

            entity.HasIndex(worker => worker.WorkerName)
                .IsUnique();
        });
    }
}