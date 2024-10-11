using Microsoft.EntityFrameworkCore;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Benefit> Benefits { get; set; }
        public DbSet<JobBenefit> JobBenefits { get; set; }
        public DbSet<WorkType> WorkTypes { get; set; }
        public DbSet<ForbiddenWord> ForbiddenWords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User tablosu
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithOne(c => c.User)
                .HasForeignKey<Company>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Company tablosu
            modelBuilder.Entity<Company>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Company>()
                .Property(c => c.Name)
                .IsRequired();

            modelBuilder.Entity<Company>()
                .Property(c => c.AllowedJobPostings)
                .IsRequired();

            modelBuilder.Entity<Job>()
                .HasKey(j => j.Id);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.Company)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.WorkType)
                .WithMany()
                .HasForeignKey(j => j.WorkTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<JobBenefit>()
                .HasKey(jb => new { jb.JobId, jb.BenefitId });

            modelBuilder.Entity<JobBenefit>()
                .HasOne(jb => jb.Job)
                .WithMany(j => j.JobBenefits)
                .HasForeignKey(jb => jb.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobBenefit>()
                .HasOne(jb => jb.Benefit)
                .WithMany()
                .HasForeignKey(jb => jb.BenefitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForbiddenWord>()
                .HasKey(fw => fw.Id);
        }
    }
}