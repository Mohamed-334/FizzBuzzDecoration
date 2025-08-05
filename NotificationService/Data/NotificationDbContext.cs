using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.Data).HasMaxLength(2000);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.ActionUrl).HasMaxLength(500);
                
                // Indexes for better query performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.UserId, e.Status });
            });

            // Configure NotificationTemplate entity
            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TitleTemplate).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MessageTemplate).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Variables).HasMaxLength(1000);
                
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
            });

            // Seed data for notification templates
            SeedNotificationTemplates(modelBuilder);
        }

        private void SeedNotificationTemplates(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationTemplate>().HasData(
                new NotificationTemplate
                {
                    Id = 1,
                    Name = "WelcomeNotification",
                    TitleTemplate = "مرحباً بك في {AppName}!",
                    MessageTemplate = "أهلاً وسهلاً {UserName}، نحن سعداء لانضمامك إلينا!",
                    Type = NotificationType.General,
                    Priority = 2,
                    Variables = "[\"AppName\", \"UserName\"]",
                    Description = "رسالة ترحيب للمستخدمين الجدد"
                },
                new NotificationTemplate
                {
                    Id = 2,
                    Name = "SystemMaintenance",
                    TitleTemplate = "صيانة النظام",
                    MessageTemplate = "سيتم إجراء صيانة للنظام من {StartTime} إلى {EndTime}. نعتذر عن أي إزعاج.",
                    Type = NotificationType.System,
                    Priority = 3,
                    Variables = "[\"StartTime\", \"EndTime\"]",
                    Description = "إشعار صيانة النظام"
                },
                new NotificationTemplate
                {
                    Id = 3,
                    Name = "SecurityAlert",
                    TitleTemplate = "تنبيه أمني",
                    MessageTemplate = "تم تسجيل دخول جديد إلى حسابك من {Location} في {DateTime}.",
                    Type = NotificationType.Security,
                    Priority = 3,
                    Variables = "[\"Location\", \"DateTime\"]",
                    Description = "تنبيه أمني لتسجيل الدخول"
                }
            );
        }
    }
}