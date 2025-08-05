using Microsoft.AspNetCore.SignalR;
using NotificationService.DTOs;

namespace NotificationService.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinUserGroup(string userId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation("User {UserId} joined notification group with connection {ConnectionId}", userId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining user {UserId} to group", userId);
                throw;
            }
        }

        public async Task LeaveUserGroup(string userId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation("User {UserId} left notification group with connection {ConnectionId}", userId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from group", userId);
                throw;
            }
        }

        public async Task MarkNotificationAsRead(int notificationId)
        {
            try
            {
                // This will be called from client to notify server that notification was read
                await Clients.Others.SendAsync("NotificationRead", notificationId);
                _logger.LogInformation("Notification {NotificationId} marked as read by connection {ConnectionId}", notificationId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                throw;
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Method to get online users count (can be called from client)
        public async Task GetOnlineUsersCount()
        {
            // This is a placeholder - in a real application you'd track connected users
            await Clients.Caller.SendAsync("OnlineUsersCount", 0);
        }
    }

    // Extension methods to make it easier to send notifications through SignalR
    public static class NotificationHubExtensions
    {
        public static async Task SendNotificationToUser(this IHubContext<NotificationHub> hubContext, string userId, NotificationDto notification)
        {
            await hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notification);
        }

        public static async Task SendNotificationToUsers(this IHubContext<NotificationHub> hubContext, List<string> userIds, NotificationDto notification)
        {
            var groups = userIds.Select(userId => $"User_{userId}").ToList();
            await hubContext.Clients.Groups(groups).SendAsync("ReceiveNotification", notification);
        }

        public static async Task SendBulkNotificationsToUsers(this IHubContext<NotificationHub> hubContext, Dictionary<string, NotificationDto> userNotifications)
        {
            foreach (var kvp in userNotifications)
            {
                await hubContext.Clients.Group($"User_{kvp.Key}").SendAsync("ReceiveNotification", kvp.Value);
            }
        }

        public static async Task SendNotificationUpdate(this IHubContext<NotificationHub> hubContext, string userId, int notificationId, string updateType, object? data = null)
        {
            await hubContext.Clients.Group($"User_{userId}").SendAsync("NotificationUpdate", new
            {
                NotificationId = notificationId,
                UpdateType = updateType,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }

        public static async Task SendUnreadCountUpdate(this IHubContext<NotificationHub> hubContext, string userId, int unreadCount)
        {
            await hubContext.Clients.Group($"User_{userId}").SendAsync("UnreadCountUpdate", unreadCount);
        }

        public static async Task BroadcastSystemNotification(this IHubContext<NotificationHub> hubContext, NotificationDto notification)
        {
            await hubContext.Clients.All.SendAsync("SystemNotification", notification);
        }
    }
}