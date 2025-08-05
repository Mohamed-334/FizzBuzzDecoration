using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NotificationsController : ControllerBase
    {
        private readonly IRealTimeNotificationService _realTimeNotificationService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            IRealTimeNotificationService realTimeNotificationService,
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _realTimeNotificationService = realTimeNotificationService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// إنشاء إشعار جديد مع إرساله في الوقت الفعلي
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notification = await _realTimeNotificationService.CreateAndSendNotificationAsync(createDto);
                return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, new { message = "خطأ في إنشاء الإشعار", error = ex.Message });
            }
        }

        /// <summary>
        /// إنشاء إشعارات متعددة للمستخدمين
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<List<NotificationDto>>> CreateBulkNotifications([FromBody] BulkNotificationDto bulkDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notifications = await _realTimeNotificationService.CreateAndSendBulkNotificationsAsync(bulkDto);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk notifications");
                return StatusCode(500, new { message = "خطأ في إنشاء الإشعارات المتعددة", error = ex.Message });
            }
        }

        /// <summary>
        /// إنشاء إشعار من قالب
        /// </summary>
        [HttpPost("from-template")]
        public async Task<ActionResult<NotificationDto>> CreateFromTemplate(
            [FromQuery] string templateName,
            [FromQuery] string userId,
            [FromBody] Dictionary<string, string> variables)
        {
            try
            {
                if (string.IsNullOrEmpty(templateName) || string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { message = "اسم القالب ومعرف المستخدم مطلوبان" });
                }

                var notification = await _realTimeNotificationService.CreateAndSendFromTemplateAsync(templateName, userId, variables);
                return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification from template {TemplateName}", templateName);
                return StatusCode(500, new { message = "خطأ في إنشاء الإشعار من القالب", error = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على إشعار بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new { message = "الإشعار غير موجود" });
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {Id}", id);
                return StatusCode(500, new { message = "خطأ في جلب الإشعار", error = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على إشعارات المستخدم
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<NotificationDto>>> GetUserNotifications(
            string userId,
            [FromQuery] NotificationFilterDto? filter = null)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId, filter);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return StatusCode(500, new { message = "خطأ في جلب إشعارات المستخدم", error = ex.Message });
            }
        }

        /// <summary>
        /// البحث في الإشعارات مع التصفية
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<object>> SearchNotifications([FromBody] NotificationFilterDto filter)
        {
            try
            {
                var (notifications, totalCount) = await _notificationService.SearchNotificationsAsync(filter);
                
                return Ok(new
                {
                    notifications,
                    totalCount,
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching notifications");
                return StatusCode(500, new { message = "خطأ في البحث في الإشعارات", error = ex.Message });
            }
        }

        /// <summary>
        /// تحديث حالة الإشعار
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateNotificationStatus(int id, [FromBody] UpdateNotificationStatusDto statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new { message = "الإشعار غير موجود" });
                }

                await _realTimeNotificationService.SendNotificationStatusUpdateAsync(notification.UserId, id, statusDto.Status);
                
                return Ok(new { message = "تم تحديث حالة الإشعار بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification {Id} status", id);
                return StatusCode(500, new { message = "خطأ في تحديث حالة الإشعار", error = ex.Message });
            }
        }

        /// <summary>
        /// تحديد الإشعار كمقروء
        /// </summary>
        [HttpPatch("{id}/mark-read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new { message = "الإشعار غير موجود" });
                }

                await _realTimeNotificationService.SendNotificationStatusUpdateAsync(notification.UserId, id, NotificationStatus.Read);
                
                return Ok(new { message = "تم تحديد الإشعار كمقروء" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {Id} as read", id);
                return StatusCode(500, new { message = "خطأ في تحديد الإشعار كمقروء", error = ex.Message });
            }
        }

        /// <summary>
        /// تحديد جميع إشعارات المستخدم كمقروءة
        /// </summary>
        [HttpPatch("user/{userId}/mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead(string userId)
        {
            try
            {
                var success = await _notificationService.MarkAllAsReadAsync(userId);
                if (!success)
                {
                    return BadRequest(new { message = "فشل في تحديد الإشعارات كمقروءة" });
                }

                await _realTimeNotificationService.SendUnreadCountUpdateAsync(userId);
                
                return Ok(new { message = "تم تحديد جميع الإشعارات كمقروءة" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                return StatusCode(500, new { message = "خطأ في تحديد جميع الإشعارات كمقروءة", error = ex.Message });
            }
        }

        /// <summary>
        /// تحديث حالة عدة إشعارات
        /// </summary>
        [HttpPatch("bulk-status")]
        public async Task<ActionResult> UpdateBulkStatus([FromBody] BulkStatusUpdateDto bulkStatusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _realTimeNotificationService.SendBulkStatusUpdateAsync(bulkStatusDto.UserId, bulkStatusDto.NotificationIds, bulkStatusDto.Status);
                
                return Ok(new { message = "تم تحديث حالة الإشعارات بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bulk notification status");
                return StatusCode(500, new { message = "خطأ في تحديث حالة الإشعارات", error = ex.Message });
            }
        }

        /// <summary>
        /// حذف إشعار
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new { message = "الإشعار غير موجود" });
                }

                await _realTimeNotificationService.SendNotificationStatusUpdateAsync(notification.UserId, id, NotificationStatus.Deleted);
                
                return Ok(new { message = "تم حذف الإشعار بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {Id}", id);
                return StatusCode(500, new { message = "خطأ في حذف الإشعار", error = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على إحصائيات إشعارات المستخدم
        /// </summary>
        [HttpGet("user/{userId}/stats")]
        public async Task<ActionResult<NotificationStatsDto>> GetUserStats(string userId)
        {
            try
            {
                var stats = await _notificationService.GetUserNotificationStatsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats for user {UserId}", userId);
                return StatusCode(500, new { message = "خطأ في جلب إحصائيات الإشعارات", error = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على عدد الإشعارات غير المقروءة
        /// </summary>
        [HttpGet("user/{userId}/unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount(string userId)
        {
            try
            {
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                return StatusCode(500, new { message = "خطأ في جلب عدد الإشعارات غير المقروءة", error = ex.Message });
            }
        }

        /// <summary>
        /// الحصول على أحدث الإشعارات
        /// </summary>
        [HttpGet("user/{userId}/recent")]
        public async Task<ActionResult<List<NotificationDto>>> GetRecentNotifications(string userId, [FromQuery] int count = 10)
        {
            try
            {
                var notifications = await _notificationService.GetRecentNotificationsAsync(userId, count);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent notifications for user {UserId}", userId);
                return StatusCode(500, new { message = "خطأ في جلب أحدث الإشعارات", error = ex.Message });
            }
        }
    }

    // DTO for bulk status update
    public class BulkStatusUpdateDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<int> NotificationIds { get; set; } = new();
        public NotificationStatus Status { get; set; }
    }
}