using NotificationService.DTOs;

namespace NotificationService.Services
{
    public interface IRealTimeNotificationService
    {
        // Send notifications with real-time updates
        Task<NotificationDto> CreateAndSendNotificationAsync(CreateNotificationDto createDto);
        Task<List<NotificationDto>> CreateAndSendBulkNotificationsAsync(BulkNotificationDto bulkDto);
        Task<NotificationDto> CreateAndSendFromTemplateAsync(string templateName, string userId, Dictionary<string, string> variables);
        
        // Real-time status updates
        Task SendNotificationStatusUpdateAsync(string userId, int notificationId, NotificationStatus status);
        Task SendUnreadCountUpdateAsync(string userId);
        Task SendBulkStatusUpdateAsync(string userId, List<int> notificationIds, NotificationStatus status);
        
        // System-wide notifications
        Task BroadcastSystemNotificationAsync(CreateNotificationDto createDto);
        Task BroadcastMaintenanceNotificationAsync(string message, DateTime? scheduledTime = null);
        
        // User connection management
        Task NotifyUserOnlineStatusAsync(string userId, bool isOnline);
        Task SendWelcomeNotificationAsync(string userId, string userName);
    }
}