using NotificationService.Models;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? Data { get; set; }
        public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; }
        public int Priority { get; set; }
    }

    public class CreateNotificationDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public NotificationType Type { get; set; } = NotificationType.General;
        
        public string? Data { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public string? ActionUrl { get; set; }
        
        [Range(1, 3)]
        public int Priority { get; set; } = 1;
    }

    public class BulkNotificationDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public List<string> UserIds { get; set; } = new();
        
        public NotificationType Type { get; set; } = NotificationType.General;
        
        public string? Data { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public string? ActionUrl { get; set; }
        
        [Range(1, 3)]
        public int Priority { get; set; } = 1;
    }

    public class UpdateNotificationStatusDto
    {
        [Required]
        public NotificationStatus Status { get; set; }
    }

    public class NotificationFilterDto
    {
        public string? UserId { get; set; }
        public NotificationType? Type { get; set; }
        public NotificationStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class NotificationStatsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadCount { get; set; }
        public int ReadCount { get; set; }
        public int ArchivedCount { get; set; }
        public Dictionary<NotificationType, int> NotificationsByType { get; set; } = new();
    }
}