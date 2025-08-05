using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class NotificationTemplate
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string TitleTemplate { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string MessageTemplate { get; set; } = string.Empty;
        
        public NotificationType Type { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public int Priority { get; set; } = 1;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? Description { get; set; }
        
        // Template variables that can be replaced
        public string? Variables { get; set; } // JSON array of variable names
    }
}