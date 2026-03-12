// Program.cs (Client)
using Medlearn.Services;
using Medlearn.Services.Implementations;
using Medlearn;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Register all services in one block
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISpecializationService, SpecializationService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILectureService, LectureService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IImageUrlService, ImageUrlService>();
builder.Services.AddScoped<IWithdrawalService, WithdrawalService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ISecureMaterialService, SecureMaterialService>();
builder.Services.AddScoped<ISecureFileViewerService, SecureFileViewerService>();
builder.Services.AddScoped<IContactService, ContactService>();

var host = builder.Build();

// Initialize auth service
try
{
    var authService = host.Services.GetRequiredService<IAuthService>();
    await authService.InitializeAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error initializing auth: {ex.Message}");
}

await host.RunAsync();
