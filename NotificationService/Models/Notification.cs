using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public NotificationType Type { get; set; }
        
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ReadAt { get; set; }
        
        public string? Data { get; set; } // JSON data for additional information
        
        public string? ImageUrl { get; set; }
        
        public string? ActionUrl { get; set; }
        
        public int Priority { get; set; } = 1; // 1 = Low, 2 = Medium, 3 = High
    }
    
    public enum NotificationType
    {
        General = 0,
        System = 1,
        Warning = 2,
        Error = 3,
        Success = 4,
        Info = 5,
        Marketing = 6,
        Security = 7
    }
    
    public enum NotificationStatus
    {
        Unread = 0,
        Read = 1,
        Archived = 2,
        Deleted = 3
    }
}