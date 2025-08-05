using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.Models;
using System.Text.Json;

namespace NotificationService.Services
{
    public class NotificationServiceImpl : INotificationService
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<NotificationServiceImpl> _logger;

        public NotificationServiceImpl(NotificationDbContext context, ILogger<NotificationServiceImpl> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createDto)
        {
            try
            {
                var notification = new Notification
                {
                    Title = createDto.Title,
                    Message = createDto.Message,
                    UserId = createDto.UserId,
                    Type = createDto.Type,
                    Data = createDto.Data,
                    ImageUrl = createDto.ImageUrl,
                    ActionUrl = createDto.ActionUrl,
                    Priority = createDto.Priority,
                    Status = NotificationStatus.Unread,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created notification {Id} for user {UserId}", notification.Id, notification.UserId);

                return MapToDto(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", createDto.UserId);
                throw;
            }
        }

        public async Task<List<NotificationDto>> CreateBulkNotificationsAsync(BulkNotificationDto bulkDto)
        {
            try
            {
                var notifications = bulkDto.UserIds.Select(userId => new Notification
                {
                    Title = bulkDto.Title,
                    Message = bulkDto.Message,
                    UserId = userId,
                    Type = bulkDto.Type,
                    Data = bulkDto.Data,
                    ImageUrl = bulkDto.ImageUrl,
                    ActionUrl = bulkDto.ActionUrl,
                    Priority = bulkDto.Priority,
                    Status = NotificationStatus.Unread,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} bulk notifications", notifications.Count);

                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk notifications");
                throw;
            }
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id);

                return notification != null ? MapToDto(notification) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {Id}", id);
                throw;
            }
        }

        public async Task<List<NotificationDto>> GetNotificationsByUserIdAsync(string userId, NotificationFilterDto? filter = null)
        {
            try
            {
                var query = _context.Notifications.Where(n => n.UserId == userId);

                if (filter != null)
                {
                    if (filter.Type.HasValue)
                        query = query.Where(n => n.Type == filter.Type.Value);

                    if (filter.Status.HasValue)
                        query = query.Where(n => n.Status == filter.Status.Value);

                    if (filter.FromDate.HasValue)
                        query = query.Where(n => n.CreatedAt >= filter.FromDate.Value);

                    if (filter.ToDate.HasValue)
                        query = query.Where(n => n.CreatedAt <= filter.ToDate.Value);
                }

                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((filter?.Page ?? 1 - 1) * (filter?.PageSize ?? 20))
                    .Take(filter?.PageSize ?? 20)
                    .ToListAsync();

                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateNotificationStatusAsync(int id, NotificationStatus status)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null) return false;

                notification.Status = status;
                if (status == NotificationStatus.Read && notification.ReadAt == null)
                {
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification status {Id}", id);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            return await UpdateNotificationStatusAsync(id, NotificationStatus.Read);
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.Status = NotificationStatus.Read;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            return await UpdateNotificationStatusAsync(id, NotificationStatus.Deleted);
        }

        public async Task<bool> ArchiveNotificationAsync(int id)
        {
            return await UpdateNotificationStatusAsync(id, NotificationStatus.Archived);
        }

        public async Task<NotificationStatsDto> GetUserNotificationStatsAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.Status != NotificationStatus.Deleted)
                    .ToListAsync();

                var stats = new NotificationStatsDto
                {
                    TotalNotifications = notifications.Count,
                    UnreadCount = notifications.Count(n => n.Status == NotificationStatus.Unread),
                    ReadCount = notifications.Count(n => n.Status == NotificationStatus.Read),
                    ArchivedCount = notifications.Count(n => n.Status == NotificationStatus.Archived),
                    NotificationsByType = notifications
                        .GroupBy(n => n.Type)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<NotificationDto>> GetRecentNotificationsAsync(string userId, int count = 10)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.Status != NotificationStatus.Deleted)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(count)
                    .ToListAsync();

                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            try
            {
                return await _context.Notifications
                    .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Unread);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<NotificationDto> CreateNotificationFromTemplateAsync(string templateName, string userId, Dictionary<string, string> variables)
        {
            try
            {
                var template = await _context.NotificationTemplates
                    .FirstOrDefaultAsync(t => t.Name == templateName && t.IsActive);

                if (template == null)
                    throw new ArgumentException($"Template '{templateName}' not found or inactive");

                var title = template.TitleTemplate;
                var message = template.MessageTemplate;

                foreach (var variable in variables)
                {
                    title = title.Replace($"{{{variable.Key}}}", variable.Value);
                    message = message.Replace($"{{{variable.Key}}}", variable.Value);
                }

                var createDto = new CreateNotificationDto
                {
                    Title = title,
                    Message = message,
                    UserId = userId,
                    Type = template.Type,
                    Priority = template.Priority,
                    ImageUrl = template.ImageUrl
                };

                return await CreateNotificationAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification from template {TemplateName} for user {UserId}", templateName, userId);
                throw;
            }
        }

        public async Task<bool> MarkMultipleAsReadAsync(List<int> notificationIds)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => notificationIds.Contains(n.Id))
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    if (notification.Status == NotificationStatus.Unread)
                    {
                        notification.Status = NotificationStatus.Read;
                        notification.ReadAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking multiple notifications as read");
                throw;
            }
        }

        public async Task<bool> DeleteMultipleNotificationsAsync(List<int> notificationIds)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => notificationIds.Contains(n.Id))
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.Status = NotificationStatus.Deleted;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple notifications");
                throw;
            }
        }

        public async Task<(List<NotificationDto> notifications, int totalCount)> SearchNotificationsAsync(NotificationFilterDto filter)
        {
            try
            {
                var query = _context.Notifications.AsQueryable();

                if (!string.IsNullOrEmpty(filter.UserId))
                    query = query.Where(n => n.UserId == filter.UserId);

                if (filter.Type.HasValue)
                    query = query.Where(n => n.Type == filter.Type.Value);

                if (filter.Status.HasValue)
                    query = query.Where(n => n.Status == filter.Status.Value);

                if (filter.FromDate.HasValue)
                    query = query.Where(n => n.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(n => n.CreatedAt <= filter.ToDate.Value);

                var totalCount = await query.CountAsync();

                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return (notifications.Select(MapToDto).ToList(), totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching notifications");
                throw;
            }
        }

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                UserId = notification.UserId,
                Type = notification.Type,
                Status = notification.Status,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                Data = notification.Data,
                ImageUrl = notification.ImageUrl,
                ActionUrl = notification.ActionUrl,
                Priority = notification.Priority
            };
        }
    }
}