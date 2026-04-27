using Microsoft.EntityFrameworkCore;
using SwiftPay.Config.Configuration;
using SwiftPay.Models;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Domain.Notification.Entities;
using SwiftPay.FXModule.Api.Models;

namespace SwiftPay.Configuration
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        // User & Role Management DbSets
        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserRole> UserRole { get; set; }

        // Customer Management DbSets
        public DbSet<CustomerProfile> Customers { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }

        // DbSets for remittance module
        public DbSet<RemittanceRequest> RemittanceRequests { get; set; }
        public DbSet<Models.RemitValidation> RemitValidations { get; set; }
        public DbSet<Models.Document> RemittanceDocuments { get; set; }

        // DbSets for audit, KYC, and notification modules
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<KYCRecord> KYCRecords { get; set; }
        public DbSet<NotificationAlert> NotificationAlerts { get; set; }


        //  (Post-Transaction)
        public DbSet<Amendment> Amendments { get; set; }
        public DbSet<Cancellation> Cancellations { get; set; }
        public DbSet<RefundRef> RefundRefs { get; set; }
        public DbSet<RemitReport> RemitReports { get; set; }

        // ===== MODULE 4.3 (FX QUOTES & FEES) =====
        // We keep these plural in C# so our Repositories don't break
        public DbSet<SwiftPay.FXModule.Api.Models.FXQuote> FXQuotes { get; set; }
        public DbSet<SwiftPay.FXModule.Api.Models.FeeRule> FeeRules { get; set; }
        public DbSet<SwiftPay.FXModule.Api.Models.RateLock> RateLocks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- TELL EF CORE TO LOOK FOR SINGULAR SQL TABLES ---
            modelBuilder.Entity<SwiftPay.FXModule.Api.Models.FXQuote>().ToTable("FXQuote");
            modelBuilder.Entity<SwiftPay.FXModule.Api.Models.FeeRule>().ToTable("FeeRule");
            modelBuilder.Entity<SwiftPay.FXModule.Api.Models.RateLock>().ToTable("RateLock");
            // ----------------------------------------------------

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Configure relationships, keys, etc.
            modelBuilder.Entity<RemitValidation>()
                .HasOne(rv => rv.RemittanceRequest)
                .WithMany(rr => rr.Validations)
                .HasForeignKey(rv => rv.RemitId);

            // Update CheckedDate to allow null values
            modelBuilder.Entity<RemitValidation>()
                .Property(rv => rv.CheckedDate)
                .IsRequired(false);
        }
    }
}


