using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Services
{
    public interface INotificationService
    {
        // Basic CRUD operations
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createDto);
        Task<List<NotificationDto>> CreateBulkNotificationsAsync(BulkNotificationDto bulkDto);
        Task<NotificationDto?> GetNotificationByIdAsync(int id);
        Task<List<NotificationDto>> GetNotificationsByUserIdAsync(string userId, NotificationFilterDto? filter = null);
        Task<bool> UpdateNotificationStatusAsync(int id, NotificationStatus status);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int id);
        Task<bool> ArchiveNotificationAsync(int id);
        
        // Statistics and analytics
        Task<NotificationStatsDto> GetUserNotificationStatsAsync(string userId);
        Task<List<NotificationDto>> GetRecentNotificationsAsync(string userId, int count = 10);
        Task<int> GetUnreadCountAsync(string userId);
        
        // Template-based notifications
        Task<NotificationDto> CreateNotificationFromTemplateAsync(string templateName, string userId, Dictionary<string, string> variables);
        
        // Bulk operations
        Task<bool> MarkMultipleAsReadAsync(List<int> notificationIds);
        Task<bool> DeleteMultipleNotificationsAsync(List<int> notificationIds);
        
        // Search and filtering
        Task<(List<NotificationDto> notifications, int totalCount)> SearchNotificationsAsync(NotificationFilterDto filter);
    }
}