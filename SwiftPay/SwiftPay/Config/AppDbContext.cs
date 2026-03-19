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
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

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

         public DbSet<FeeRule> FeeRules { get; set; }

         public DbSet<FXQuote> FXQuotes { get; set; }

         public DbSet<RateLock> RateLocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
