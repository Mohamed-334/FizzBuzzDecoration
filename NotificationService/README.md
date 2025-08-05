# 🔔 Notification Service - مايكروسيرفيس الإشعارات

مايكروسيرفيس متكامل للإشعارات مبني بـ ASP.NET Core Web API مع دعم التحديثات المباشرة باستخدام SignalR.

## ✨ المميزات الرئيسية

- **🚀 إشعارات مباشرة**: تحديثات فورية باستخدام SignalR
- **📝 قوالب الإشعارات**: نظام قوالب مرن لإنشاء الإشعارات
- **📊 إحصائيات شاملة**: تتبع حالة الإشعارات والإحصائيات
- **🔍 بحث وتصفية**: إمكانيات بحث متقدمة
- **📱 إشعارات متعددة**: دعم الإشعارات الجماعية
- **🎯 أنواع مختلفة**: دعم أنواع متعددة من الإشعارات
- **📚 توثيق تفاعلي**: Swagger UI للـ API

## 🏗️ البنية المعمارية

```
NotificationService/
├── Controllers/          # API Controllers
├── Services/            # Business Logic Services
├── Models/              # Data Models
├── DTOs/               # Data Transfer Objects
├── Data/               # Database Context
├── Hubs/               # SignalR Hubs
└── Program.cs          # Application Entry Point
```

## 🛠️ التقنيات المستخدمة

- **ASP.NET Core 8.0** - Web API Framework
- **SignalR** - Real-time Communication
- **Entity Framework Core** - ORM
- **In-Memory Database** - للتطوير والاختبار
- **Swagger/OpenAPI** - API Documentation
- **CORS** - Cross-Origin Resource Sharing

## 🚀 التشغيل السريع

### المتطلبات الأساسية
- .NET 8.0 SDK
- Visual Studio 2022 أو VS Code

### خطوات التشغيل

1. **استنساخ المشروع**
```bash
cd NotificationService
```

2. **استعادة الحزم**
```bash
dotnet restore
```

3. **تشغيل التطبيق**
```bash
dotnet run
```

4. **الوصول للتطبيق**
- API Documentation: `https://localhost:5001`
- SignalR Hub: `https://localhost:5001/notificationHub`
- Health Check: `https://localhost:5001/health`

## 📡 API Endpoints

### الإشعارات الأساسية
- `POST /api/notifications` - إنشاء إشعار جديد
- `GET /api/notifications/{id}` - الحصول على إشعار
- `GET /api/notifications/user/{userId}` - إشعارات المستخدم
- `PATCH /api/notifications/{id}/mark-read` - تحديد كمقروء
- `DELETE /api/notifications/{id}` - حذف إشعار

### الإشعارات المتعددة
- `POST /api/notifications/bulk` - إنشاء إشعارات متعددة
- `PATCH /api/notifications/bulk-status` - تحديث حالة متعددة

### القوالب والإحصائيات
- `POST /api/notifications/from-template` - إنشاء من قالب
- `GET /api/notifications/user/{userId}/stats` - إحصائيات المستخدم
- `GET /api/notifications/user/{userId}/unread-count` - عدد غير المقروءة

### الإدارة
- `POST /api/admin/broadcast-system` - إشعار نظام عام
- `POST /api/admin/broadcast-maintenance` - إشعار صيانة
- `POST /api/admin/welcome-user` - رسالة ترحيب

## 🔌 SignalR Events

### الأحداث الواردة (من العميل)
```javascript
// الانضمام لمجموعة المستخدم
connection.invoke("JoinUserGroup", userId);

// الخروج من مجموعة المستخدم
connection.invoke("LeaveUserGroup", userId);

// تحديد إشعار كمقروء
connection.invoke("MarkNotificationAsRead", notificationId);
```

### الأحداث الصادرة (للعميل)
```javascript
// استقبال إشعار جديد
connection.on("ReceiveNotification", (notification) => {
    console.log("New notification:", notification);
});

// تحديث عدد غير المقروءة
connection.on("UnreadCountUpdate", (count) => {
    console.log("Unread count:", count);
});

// تحديث حالة الإشعار
connection.on("NotificationUpdate", (update) => {
    console.log("Notification update:", update);
});

// إشعار نظام
connection.on("SystemNotification", (notification) => {
    console.log("System notification:", notification);
});
```

## 📊 نماذج البيانات

### Notification
```csharp
public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Data { get; set; }
    public string? ImageUrl { get; set; }
    public string? ActionUrl { get; set; }
    public int Priority { get; set; }
}
```

### أنواع الإشعارات
```csharp
public enum NotificationType
{
    General = 0,    // عام
    System = 1,     // نظام
    Warning = 2,    // تحذير
    Error = 3,      // خطأ
    Success = 4,    // نجاح
    Info = 5,       // معلومات
    Marketing = 6,  // تسويق
    Security = 7    // أمان
}
```

### حالة الإشعارات
```csharp
public enum NotificationStatus
{
    Unread = 0,    // غير مقروء
    Read = 1,      // مقروء
    Archived = 2,  // مؤرشف
    Deleted = 3    // محذوف
}
```

## 📝 أمثلة الاستخدام

### إنشاء إشعار جديد
```bash
curl -X POST "https://localhost:5001/api/notifications" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "مرحباً بك",
    "message": "أهلاً وسهلاً في النظام",
    "userId": "user123",
    "type": 0,
    "priority": 2
  }'
```

### إنشاء إشعارات متعددة
```bash
curl -X POST "https://localhost:5001/api/notifications/bulk" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "إشعار جماعي",
    "message": "رسالة لجميع المستخدمين",
    "userIds": ["user1", "user2", "user3"],
    "type": 1,
    "priority": 3
  }'
```

### الحصول على إشعارات المستخدم
```bash
curl "https://localhost:5001/api/notifications/user/user123?page=1&pageSize=10"
```

## 🧪 بيانات تجريبية

في بيئة التطوير، يمكنك إنشاء بيانات تجريبية:

```bash
curl -X POST "https://localhost:5001/api/seed-test-data"
```

## 🔧 التكوين

### إعدادات قاعدة البيانات
```csharp
// في Program.cs
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseInMemoryDatabase("NotificationDb"));
```

### إعدادات CORS للـ SignalR
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## 🏥 Health Checks

يتضمن المشروع فحص صحة النظام:
- `GET /health` - حالة النظام وقاعدة البيانات

## 📈 الإحصائيات والمراقبة

### إحصائيات المستخدم
```json
{
  "totalNotifications": 25,
  "unreadCount": 5,
  "readCount": 18,
  "archivedCount": 2,
  "notificationsByType": {
    "General": 10,
    "System": 8,
    "Security": 7
  }
}
```

## 🔮 التطوير المستقبلي

- [ ] دعم قواعد البيانات الخارجية (SQL Server, PostgreSQL)
- [ ] نظام المصادقة والتفويض
- [ ] إشعارات البريد الإلكتروني والـ SMS
- [ ] واجهة إدارة ويب
- [ ] API للتطبيقات المحمولة
- [ ] نظام الاشتراكات والموضوعات
- [ ] تحليلات متقدمة ولوحة معلومات

## 🤝 المساهمة

نرحب بالمساهمات! يرجى:
1. Fork المشروع
2. إنشاء branch للميزة الجديدة
3. Commit التغييرات
4. Push للـ branch
5. إنشاء Pull Request

## 📄 الترخيص

هذا المشروع مرخص تحت رخصة MIT - راجع ملف [LICENSE](LICENSE) للتفاصيل.

## 📞 الدعم

للدعم والاستفسارات:
- إنشاء Issue في GitHub
- مراجعة الوثائق التفاعلية في `/swagger`
- فحص Health Check في `/health`

---
**تم التطوير بـ ❤️ باستخدام ASP.NET Core و SignalR**