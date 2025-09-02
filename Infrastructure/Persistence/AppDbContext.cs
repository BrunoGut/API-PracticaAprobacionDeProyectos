using Domain.Entities;
using Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Area> Areas { get; set; }
        public DbSet<ProjectType> ProjectTypes { get; set; }
        public DbSet<ApprovalStatus> ApprovalStatuses { get; set; }
        public DbSet<ApproverRole> ApproverRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ApprovalRule> ApprovalRules { get; set; }
        public DbSet<ProjectProposal> ProjectProposals { get; set; }
        public DbSet<ProjectApprovalStep> ProjectApprovalSteps { get; set; }




        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-O1PN00U\\SQLEXPRESS;Database=AprobacionProyectosDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("Area");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("int").IsRequired();
                entity.Property(e => e.Name).IsRequired().HasColumnType("varchar(25)");
                //relacion con ApprovalRule
                entity.HasMany(e => e.ApprovalRules)
                .WithOne(ar => ar.ProjectArea)
                .HasForeignKey(ar => ar.Area);
                //relacion con ProjectProposal
                entity.HasMany(e => e.ProjectProposals)
                .WithOne(pp => pp.ProjectArea)
                .HasForeignKey(pp => pp.Area).IsRequired();
            });
            modelBuilder.ApplyConfiguration(new AreaConfiguration());

            modelBuilder.Entity<ProjectType>(entity =>
            {
                entity.ToTable("ProjectType");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("int").IsRequired();
                entity.Property(e => e.Name).HasColumnType("varchar(25)").IsRequired();
                //relacion con ApprovalRule
                entity.HasMany(e => e.ApprovalRules)
                .WithOne(ar => ar.ProjectType)
                .HasForeignKey(ar => ar.Type);
                //relacion con ProjectProposal
                entity.HasMany(e => e.ProjectProposals)
                .WithOne(pp => pp.ProjectType)
                .HasForeignKey(pp => pp.Type).IsRequired();
            });
            modelBuilder.ApplyConfiguration(new ProjectTypeConfiguration());

            modelBuilder.Entity<ApprovalStatus>(entity =>
            {
                entity.ToTable("ApprovalStatus");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("int").IsRequired();
                entity.Property(e => e.Name).HasColumnType("varchar(25)").IsRequired();
                //relacion con ProjectProposal
                entity.HasMany(e => e.ProjectProposals)
                .WithOne(pp => pp.ApprovalStatus)
                .HasForeignKey(pp => pp.Status).IsRequired();
                //relacion con ProjectApprovalStep
                entity.HasMany(e => e.ProjectApprovalSteps)
                .WithOne(pas => pas.ApprovalStatus)
                .HasForeignKey(pas => pas.Status).IsRequired();
            });
            modelBuilder.ApplyConfiguration(new ApprovalStatusConfiguration());

            modelBuilder.Entity<ApproverRole>(entity =>
            {
                entity.ToTable("ApproverRole");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("int").IsRequired();
                entity.Property(e => e.Name).HasColumnType("varchar(25)").IsRequired();
                //relacion con ProjectApprovalStep
                entity.HasMany(e => e.ProjectApprovalSteps)
                .WithOne(pas => pas.ApproverRole)
                .HasForeignKey(pas => pas.ApproverRoleId).IsRequired();
                //relacion con User
                entity.HasMany(e => e.Users)
                .WithOne(u => u.ApproverRole)
                .HasForeignKey(u => u.Role).IsRequired();
                //relacion con ApprovalRule
                entity.HasMany(e => e.ApprovalRules)
                .WithOne(apr => apr.ApproverRole)
                .HasForeignKey(apr => apr.ApproverRoleId).IsRequired();
            });
            modelBuilder.ApplyConfiguration(new ApproverRoleConfiguration());

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("int").IsRequired();
                entity.Property(e => e.Name).HasColumnType("varchar(25)").IsRequired();
                entity.Property(e => e.Email).HasColumnType("varchar(100)").IsRequired();
                //relacion con ProjectProposal
                entity.HasMany(e => e.ProjectProposals)
                .WithOne(pp => pp.User)
                .HasForeignKey(pp => pp.CreateBy).IsRequired();
                //relacion con ApproverRule
                entity.HasOne(e => e.ApproverRole)
                .WithMany(ar => ar.Users)
                .HasForeignKey(e => e.Role).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.Entity<ApprovalRule>(entity =>
            {
                entity.ToTable("ApprovalRule");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("bigint")
                .ValueGeneratedOnAdd().IsRequired();
                entity.Property(e => e.MinAmount).IsRequired()
                .HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxAmount).IsRequired()
                .HasColumnType("decimal(18,2)");
                entity.Property(e => e.StepOrder).HasColumnType("int").IsRequired();
                //relacion con Area
                entity.HasOne(e => e.ProjectArea)
                .WithMany(a => a.ApprovalRules)
                .HasForeignKey(e => e.Area)
                .OnDelete(DeleteBehavior.Restrict);
                //relacion con ProjectType
                entity.HasOne(e => e.ProjectType)
                .WithMany(t => t.ApprovalRules)
                .HasForeignKey(e => e.Type)
                .OnDelete(DeleteBehavior.Restrict);
                //relacion con ApproverRole
                entity.HasOne(e => e.ApproverRole)
                .WithMany(a => a.ApprovalRules)
                .HasForeignKey(e => e.ApproverRoleId).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.ApplyConfiguration(new ApprovalRuleConfiguration());

            modelBuilder.Entity<ProjectProposal>(entity =>
            {
                entity.ToTable("ProjectProposal");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnType("uniqueidentifier")
                .ValueGeneratedOnAdd().IsRequired();
                entity.Property(e => e.Title).IsRequired()
                .HasColumnType("varchar(255)");
                entity.Property(e => e.Description).IsRequired()
                .HasColumnType("varchar(max)");
                entity.Property(e => e.CreateAt).HasColumnType("datetime").IsRequired();
                //relacion con Area
                entity.HasOne(e => e.ProjectArea)
                .WithMany(a => a.ProjectProposals)
                .HasForeignKey(e => e.Area).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                //relacion con ProjectType
                entity.HasOne(e => e.ProjectType)
                .WithMany(pt => pt.ProjectProposals)
                .HasForeignKey(e => e.Type).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                //relacion con ApprovalStatus
                entity.HasOne(e => e.ApprovalStatus)
                .WithMany(aps => aps.ProjectProposals)
                .HasForeignKey(e => e.Status).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                //relacion con User
                entity.HasOne(e => e.User)
                .WithMany(u => u.ProjectProposals)
                .HasForeignKey(e => e.CreateBy).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProjectApprovalStep>(entity =>
            {
                entity.ToTable("ProjectApprovalStep");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
                entity.Property(e => e.StepOrder).IsRequired().HasColumnType("int");
                entity.Property(e => e.DecisionDate).HasColumnType("datetime");
                entity.Property(e => e.Observations).HasColumnType("varchar(max)");
                //relacion con ProjectProposal
                entity.HasOne(e => e.ProjectProposal)
                .WithMany(pp => pp.ProjectApprovalSteps)
                .HasForeignKey(e => e.ProjectProposalId).IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
                //relacion con User
                entity.HasOne(e => e.User)
                .WithMany(u => u.ProjectApprovalSteps)
                .HasForeignKey(e => e.ApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);
                //relacion con ApproverRole
                entity.HasOne(e => e.ApproverRole)
                .WithMany(apr => apr.ProjectApprovalSteps)
                .HasForeignKey(e => e.ApproverRoleId).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                //relacion con ApprovalStatus
                entity.HasOne(e => e.ApprovalStatus)
                .WithMany(aps => aps.ProjectApprovalSteps)
                .HasForeignKey(e => e.Status).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
