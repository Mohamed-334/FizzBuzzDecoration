using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IRealTimeNotificationService _realTimeNotificationService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IRealTimeNotificationService realTimeNotificationService,
            ILogger<AdminController> logger)
        {
            _realTimeNotificationService = realTimeNotificationService;
            _logger = logger;
        }

        /// <summary>
        /// إرسال إشعار نظام لجميع المستخدمين
        /// </summary>
        [HttpPost("broadcast-system")]
        public async Task<ActionResult> BroadcastSystemNotification([FromBody] CreateNotificationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _realTimeNotificationService.BroadcastSystemNotificationAsync(createDto);
                
                return Ok(new { message = "تم إرسال إشعار النظام بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting system notification");
                return StatusCode(500, new { message = "خطأ في إرسال إشعار النظام", error = ex.Message });
            }
        }

        /// <summary>
        /// إرسال إشعار صيانة
        /// </summary>
        [HttpPost("broadcast-maintenance")]
        public async Task<ActionResult> BroadcastMaintenanceNotification([FromBody] MaintenanceNotificationDto maintenanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _realTimeNotificationService.BroadcastMaintenanceNotificationAsync(maintenanceDto.Message, maintenanceDto.ScheduledTime);
                
                return Ok(new { message = "تم إرسال إشعار الصيانة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting maintenance notification");
                return StatusCode(500, new { message = "خطأ في إرسال إشعار الصيانة", error = ex.Message });
            }
        }

        /// <summary>
        /// إرسال رسالة ترحيب لمستخدم جديد
        /// </summary>
        [HttpPost("welcome-user")]
        public async Task<ActionResult> SendWelcomeNotification([FromBody] WelcomeNotificationDto welcomeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _realTimeNotificationService.SendWelcomeNotificationAsync(welcomeDto.UserId, welcomeDto.UserName);
                
                return Ok(new { message = "تم إرسال رسالة الترحيب بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome notification to user {UserId}", welcomeDto.UserId);
                return StatusCode(500, new { message = "خطأ في إرسال رسالة الترحيب", error = ex.Message });
            }
        }

        /// <summary>
        /// تحديث حالة اتصال المستخدم
        /// </summary>
        [HttpPost("user-online-status")]
        public async Task<ActionResult> UpdateUserOnlineStatus([FromBody] UserOnlineStatusDto statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _realTimeNotificationService.NotifyUserOnlineStatusAsync(statusDto.UserId, statusDto.IsOnline);
                
                return Ok(new { message = "تم تحديث حالة الاتصال بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user online status for {UserId}", statusDto.UserId);
                return StatusCode(500, new { message = "خطأ في تحديث حالة الاتصال", error = ex.Message });
            }
        }
    }

    // DTOs for admin operations
    public class MaintenanceNotificationDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime? ScheduledTime { get; set; }
    }

    public class WelcomeNotificationDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    public class UserOnlineStatusDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
    }
}