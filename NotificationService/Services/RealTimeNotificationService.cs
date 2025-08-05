using Microsoft.AspNetCore.SignalR;
using NotificationService.DTOs;
using NotificationService.Hubs;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<RealTimeNotificationService> _logger;

        public RealTimeNotificationService(
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext,
            ILogger<RealTimeNotificationService> logger)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<NotificationDto> CreateAndSendNotificationAsync(CreateNotificationDto createDto)
        {
            try
            {
                // Create notification in database
                var notification = await _notificationService.CreateNotificationAsync(createDto);

                // Send real-time notification to user
                await _hubContext.SendNotificationToUser(notification.UserId, notification);

                // Update unread count
                await SendUnreadCountUpdateAsync(notification.UserId);

                _logger.LogInformation("Created and sent real-time notification {Id} to user {UserId}", 
                    notification.Id, notification.UserId);

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating and sending notification to user {UserId}", createDto.UserId);
                throw;
            }
        }

        public async Task<List<NotificationDto>> CreateAndSendBulkNotificationsAsync(BulkNotificationDto bulkDto)
        {
            try
            {
                // Create bulk notifications in database
                var notifications = await _notificationService.CreateBulkNotificationsAsync(bulkDto);

                // Send real-time notifications to all users
                var userNotifications = notifications.ToDictionary(n => n.UserId, n => n);
                await _hubContext.SendBulkNotificationsToUsers(userNotifications);

                // Update unread counts for all users
                foreach (var userId in bulkDto.UserIds)
                {
                    await SendUnreadCountUpdateAsync(userId);
                }

                _logger.LogInformation("Created and sent {Count} bulk real-time notifications", notifications.Count);

                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating and sending bulk notifications");
                throw;
            }
        }

        public async Task<NotificationDto> CreateAndSendFromTemplateAsync(string templateName, string userId, Dictionary<string, string> variables)
        {
            try
            {
                // Create notification from template
                var notification = await _notificationService.CreateNotificationFromTemplateAsync(templateName, userId, variables);

                // Send real-time notification to user
                await _hubContext.SendNotificationToUser(notification.UserId, notification);

                // Update unread count
                await SendUnreadCountUpdateAsync(notification.UserId);

                _logger.LogInformation("Created and sent template-based notification {Id} from template {TemplateName} to user {UserId}", 
                    notification.Id, templateName, userId);

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating and sending template notification {TemplateName} to user {UserId}", templateName, userId);
                throw;
            }
        }

        public async Task SendNotificationStatusUpdateAsync(string userId, int notificationId, NotificationStatus status)
        {
            try
            {
                // Update status in database
                await _notificationService.UpdateNotificationStatusAsync(notificationId, status);

                // Send real-time status update
                await _hubContext.SendNotificationUpdate(userId, notificationId, "StatusUpdate", new { Status = status });

                // Update unread count if status changed to read
                if (status == NotificationStatus.Read)
                {
                    await SendUnreadCountUpdateAsync(userId);
                }

                _logger.LogInformation("Updated notification {Id} status to {Status} for user {UserId}", 
                    notificationId, status, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification {Id} status for user {UserId}", notificationId, userId);
                throw;
            }
        }

        public async Task SendUnreadCountUpdateAsync(string userId)
        {
            try
            {
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                await _hubContext.SendUnreadCountUpdate(userId, unreadCount);

                _logger.LogDebug("Sent unread count update {Count} to user {UserId}", unreadCount, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending unread count update to user {UserId}", userId);
                throw;
            }
        }

        public async Task SendBulkStatusUpdateAsync(string userId, List<int> notificationIds, NotificationStatus status)
        {
            try
            {
                // Update multiple notifications status
                if (status == NotificationStatus.Read)
                {
                    await _notificationService.MarkMultipleAsReadAsync(notificationIds);
                }
                else if (status == NotificationStatus.Deleted)
                {
                    await _notificationService.DeleteMultipleNotificationsAsync(notificationIds);
                }

                // Send real-time bulk status update
                await _hubContext.SendNotificationUpdate(userId, 0, "BulkStatusUpdate", new 
                { 
                    NotificationIds = notificationIds, 
                    Status = status 
                });

                // Update unread count
                await SendUnreadCountUpdateAsync(userId);

                _logger.LogInformation("Updated {Count} notifications status to {Status} for user {UserId}", 
                    notificationIds.Count, status, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bulk notification status for user {UserId}", userId);
                throw;
            }
        }

        public async Task BroadcastSystemNotificationAsync(CreateNotificationDto createDto)
        {
            try
            {
                // Create system notification (assuming it's for all users or specific users)
                var notification = await _notificationService.CreateNotificationAsync(createDto);

                // Broadcast to all connected clients
                await _hubContext.BroadcastSystemNotification(notification);

                _logger.LogInformation("Broadcasted system notification {Id}", notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting system notification");
                throw;
            }
        }

        public async Task BroadcastMaintenanceNotificationAsync(string message, DateTime? scheduledTime = null)
        {
            try
            {
                var maintenanceNotification = new NotificationDto
                {
                    Id = 0, // Temporary ID for broadcast
                    Title = "إشعار صيانة النظام",
                    Message = message,
                    Type = NotificationType.System,
                    Priority = 3,
                    CreatedAt = DateTime.UtcNow,
                    Status = NotificationStatus.Unread,
                    UserId = "system",
                    Data = scheduledTime.HasValue ? $"{{\"scheduledTime\": \"{scheduledTime:yyyy-MM-dd HH:mm:ss}\"}}" : null
                };

                await _hubContext.BroadcastSystemNotification(maintenanceNotification);

                _logger.LogInformation("Broadcasted maintenance notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting maintenance notification");
                throw;
            }
        }

        public async Task NotifyUserOnlineStatusAsync(string userId, bool isOnline)
        {
            try
            {
                await _hubContext.Clients.Group($"User_{userId}").SendAsync("UserOnlineStatus", new 
                { 
                    UserId = userId, 
                    IsOnline = isOnline, 
                    Timestamp = DateTime.UtcNow 
                });

                _logger.LogDebug("Notified user {UserId} online status: {IsOnline}", userId, isOnline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying user {UserId} online status", userId);
                throw;
            }
        }

        public async Task SendWelcomeNotificationAsync(string userId, string userName)
        {
            try
            {
                var variables = new Dictionary<string, string>
                {
                    { "UserName", userName },
                    { "AppName", "نظام الإشعارات" }
                };

                await CreateAndSendFromTemplateAsync("WelcomeNotification", userId, variables);

                _logger.LogInformation("Sent welcome notification to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome notification to user {UserId}", userId);
                throw;
            }
        }
    }
}