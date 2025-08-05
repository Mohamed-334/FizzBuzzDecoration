using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework with In-Memory Database
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseInMemoryDatabase("NotificationDb"));

// Add SignalR
builder.Services.AddSignalR();

// Register application services
builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();
builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();

// Add CORS policy for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5173", "https://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Notification Service API", 
        Version = "v1",
        Description = "مايكروسيرفيس الإشعارات - يوفر خدمات إدارة الإشعارات مع التحديثات المباشرة باستخدام SignalR"
    });
    
    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotificationDbContext>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("SignalRPolicy");

app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

// Add health check endpoint
app.MapHealthChecks("/health");

// Add some basic endpoints for testing
app.MapGet("/", () => "Notification Service is running! 🚀");

app.MapGet("/api/status", () => new
{
    Service = "Notification Service",
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Features = new[]
    {
        "Real-time notifications with SignalR",
        "Template-based notifications",
        "Bulk notification support",
        "User notification statistics",
        "RESTful API endpoints"
    }
});

// Seed some test data in development
if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/seed-test-data", async (IServiceProvider serviceProvider) =>
    {
        using var scope = serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        
        // Create some test notifications
        var testNotifications = new[]
        {
            new CreateNotificationDto
            {
                Title = "مرحباً بك في النظام",
                Message = "نحن سعداء لانضمامك إلى نظام الإشعارات",
                UserId = "user1",
                Type = Models.NotificationType.General,
                Priority = 2
            },
            new CreateNotificationDto
            {
                Title = "تحديث النظام",
                Message = "تم تحديث النظام بنجاح إلى الإصدار الجديد",
                UserId = "user1",
                Type = Models.NotificationType.System,
                Priority = 1
            },
            new CreateNotificationDto
            {
                Title = "تنبيه أمني",
                Message = "تم تسجيل دخول جديد إلى حسابك",
                UserId = "user2",
                Type = Models.NotificationType.Security,
                Priority = 3
            }
        };

        foreach (var notification in testNotifications)
        {
            await notificationService.CreateNotificationAsync(notification);
        }

        return Results.Ok(new { message = "تم إنشاء بيانات تجريبية بنجاح", count = testNotifications.Length });
    });
}

Console.WriteLine("🚀 Notification Service is starting...");
Console.WriteLine($"📱 SignalR Hub available at: /notificationHub");
Console.WriteLine($"📚 API Documentation available at: /swagger (in development)");
Console.WriteLine($"🔍 Health check available at: /health");

app.Run();
