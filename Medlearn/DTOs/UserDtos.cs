// DTOs/UserDtos.cs
using Medlearn.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Medlearn.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public UserType UserType { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
        public ProfileDto? Profile { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
    public class ProgressUpdateEventArgs
    {
        public bool IsCompleted { get; set; }
        public int TimeSpentSeconds { get; set; }
        public int LastPositionSeconds { get; set; }
        public int MaterialId { get; set; } // Add this line
    }
    public class ChatMessage
    {
        public string Sender { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsBot { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType UserType { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Bio { get; set; }
    }

    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public UserType? UserType { get; set; } // Add this line

    }

    public class ProfileDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Department { get; set; }
        public string? Institution { get; set; }
        public string? Expertise { get; set; }
        public string? LearningGoals { get; set; }
        public string? Interests { get; set; }
        public string? EducationBackground { get; set; }
        public decimal Balance { get; set; }
        public int TotalStudents { get; set; }
        public decimal RatingAverage { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? Title { get; set; }
        public string? Department { get; set; }
        public string? Institution { get; set; }
        public string? Expertise { get; set; }
        public string? LearningGoals { get; set; }
        public string? Interests { get; set; }
        public string? EducationBackground { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class DoctorDashboardDto
    {
        public DoctorProfileDto Profile { get; set; }
        public int TotalEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<TopMaterialDto> TopMaterials { get; set; } = new();
        public List<RecentReviewDto> RecentReviews { get; set; } = new();
    }

    public class DoctorProfileDto
    {
        public decimal Balance { get; set; }
        public int TotalStudents { get; set; }
        public decimal RatingAverage { get; set; }
    }

    public class TopMaterialDto
    {
        public int Type { get; set; } // 1=Lecture, 2=Course, 3=Specialization
        public int Id { get; set; }
        public string Title { get; set; }
        public int EnrollmentCount { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class RecentReviewDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public int Type { get; set; } // 1=Lecture, 2=Course, 3=Specialization
        public string EntityTitle { get; set; }
    }

    // User dashboard DTOs
    public class UserDashboardDto
    {
        public int TotalEnrollments { get; set; }
        public int InProgressEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public decimal AverageProgress { get; set; }
        public List<UserEnrollmentDto> RecentEnrollments { get; set; } = new();
        public int PendingPayments { get; set; }
        public int UnreadNotifications { get; set; }
    }

    public class UserEnrollmentDto
    {
        public int Id { get; set; }
        public int Type { get; set; } // 1=Lecture, 2=Course, 3=Specialization
        public string Title { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public decimal ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
    }

   
}

// DTOs/CourseDtos.cs
namespace Medlearn.DTOs
{
    public class SpecializationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public int EstimatedDurationHours { get; set; }
        public string? Prerequisites { get; set; }
        public int? DoctorId { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CourseDto> Courses { get; set; } = new();
        public UserDto? Doctor { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal AverageRating { get; set; }
        public bool IsActive { get; set; } // Add this

    }

    public class CreateSpecializationDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int EstimatedDurationHours { get; set; }
        public string? Prerequisites { get; set; }
        public IFormFile? ThumbnailImage { get; set; }
        public bool IsFeatured { get; set; }
    }

    public class UpdateSpecializationDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? EstimatedDurationHours { get; set; }
        public string? Prerequisites { get; set; }
        public IFormFile? ThumbnailImage { get; set; }
        public bool? IsFeatured { get; set; }
    }
    public class UpdateSpecializationDtos
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? EstimatedDurationHours { get; set; }
        public string? Prerequisites { get; set; }
        public IBrowserFile? ThumbnailImage { get; set; }
        public bool? IsFeatured { get; set; }
        public bool IsActive { get; set; } // Add this

    }

    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public int Order { get; set; }
        public int SpecializationId { get; set; }
        public int? DoctorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<LectureDto> Lectures { get; set; } = new();
        public UserDto? Doctor { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal AverageRating { get; set; }
        public bool IsActive { get; set; } // Add this

    }

    public class CreateCourseDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Order { get; set; }
        public int SpecializationId { get; set; }
        public IFormFile? ThumbnailImage { get; set; }
        public string? ThumbnailUrl { get; set; }
    }


    public class UpdateCourseDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Order { get; set; }
        public IFormFile? ThumbnailImage { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
    public class CreateCourseRequestDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Order { get; set; }
        public int SpecializationId { get; set; }
        public IBrowserFile? ThumbnailImage { get; set; }
        public string? ThumbnailUrl { get; set; }

    }

    public class UpdateCourseRequestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Order { get; set; }
        public IBrowserFile? ThumbnailImage { get; set; }
        public string? ThumbnailUrl { get; set; }

    }

    public class LectureDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public int CourseId { get; set; }
        public int? DoctorId { get; set; }
        public decimal Price { get; set; }
        public bool IsFreePreview { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<LearningMaterialDto> Materials { get; set; } = new();
        public UserDto? Doctor { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal AverageRating { get; set; }
        public bool IsActive { get; set; } // Add this

    }

    public class CreateLectureDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public int CourseId { get; set; }
        public decimal Price { get; set; }
        public bool IsFreePreview { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public IFormFile? ThumbnailImage { get; set; }
        public List<MaterialUploadDto> Materials { get; set; } = new();
    }
    public class CreateLectureDtos
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public int CourseId { get; set; }
        public decimal Price { get; set; }
        public bool IsFreePreview { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public IBrowserFile? ThumbnailImage { get; set; }
        public List<MaterialUploadDtos> Materials { get; set; } = new();
    }

    public class UpdateLectureDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? Order { get; set; }
        public decimal? Price { get; set; }
        public bool? IsFreePreview { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public IFormFile? ThumbnailImage { get; set; }
        public List<MaterialUploadDto> Materials { get; set; } = new();
        public bool IsActive { get; set; } // Add this



    }
    public class UpdateLectureDtos
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? Order { get; set; }
        public decimal? Price { get; set; }
        public bool? IsFreePreview { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public IBrowserFile? ThumbnailImage { get; set; }
        public List<MaterialUploadDto> Materials { get; set; } = new();
        public bool IsActive { get; set; } // Add this


    }

    public class LearningMaterialDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public MaterialType Type { get; set; }
        public string ContentUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int DurationSeconds { get; set; }
        public long FileSize { get; set; }
        public int LectureId { get; set; }
        public int Order { get; set; }
        public bool IsDownloadable { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class MaterialUploadDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public MaterialType Type { get; set; }
        public IFormFile File { get; set; }
        public bool IsDownloadable { get; set; }
        public int Order { get; set; }
    }
    public class MaterialUploadDtos
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public MaterialType Type { get; set; }
        public IBrowserFile File { get; set; }
        public bool IsDownloadable { get; set; }
        public int DurationSeconds { get; set; }
        public long FileSize { get; set; }
        public int Order { get; set; }
    }
    public class MaterialInfoDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public MaterialType Type { get; set; }
        public string ContentUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public int DurationSeconds { get; set; }
        public long FileSize { get; set; }
        public int LectureId { get; set; }
        public string LectureTitle { get; set; }
        public bool IsFreePreview { get; set; }
        public string CourseTitle { get; set; }
        public string SpecializationTitle { get; set; }
        public int Order { get; set; }
        public bool IsDownloadable { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed properties
        public string DurationFormatted =>
            TimeSpan.FromSeconds(DurationSeconds).ToString(@"hh\:mm\:ss");
        public string FileSizeFormatted => FormatFileSize(FileSize);

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
    // In your DTOs namespace
    public class MaterialDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public MaterialType Type { get; set; }
        public string ContentUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public int DurationSeconds { get; set; }
        public long FileSize { get; set; }
        public int LectureId { get; set; }
        public int Order { get; set; }
        public bool IsDownloadable { get; set; }
        public DateTime CreatedAt { get; set; }

        // Helper properties for frontend
        public string DurationFormatted => TimeSpan.FromSeconds(DurationSeconds).ToString(@"hh\:mm\:ss");
        public string FileSizeFormatted => FormatFileSize(FileSize);

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}

// DTOs/EnrollmentDtos.cs
namespace Medlearn.DTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public EnrollmentType EnrollmentType { get; set; }
        public int? LectureId { get; set; }
        public int? CourseId { get; set; }
        public int? SpecializationId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal PricePaid { get; set; }
        public bool IsCompleted { get; set; }
        public double ProgressPercentage { get; set; }
        public UserDto User { get; set; }
        public LectureDto? Lecture { get; set; }
        public CourseDto? Course { get; set; }
        public SpecializationDto? Specialization { get; set; }
        public List<ProgressTrackingDto> ProgressTrackings { get; set; } = new();
        public ReviewDto? Review { get; set; }
    }

    public class CreateEnrollmentDto
    {
        public EnrollmentType EnrollmentType { get; set; }
        public int? LectureId { get; set; }
        public int? CourseId { get; set; }
        public int? SpecializationId { get; set; }
        public int? UserId { get; set; }
    }

    public class ProgressTrackingDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public int MaterialId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int TimeSpentSeconds { get; set; }
        public int LastPositionSeconds { get; set; }
        public DateTime CreatedAt { get; set; } // Add this
        public DateTime? UpdatedAt { get; set; } // Add this
        public LearningMaterialDto Material { get; set; }
    }

    public class UpdateProgressDto
    {
        public int MaterialId { get; set; }
        public bool IsCompleted { get; set; }
        public int TimeSpentSeconds { get; set; }
        public int LastPositionSeconds { get; set; }
    }
}

// DTOs/PaymentDtos.cs
namespace Medlearn.DTOs
{
    public class PaymentRequestDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public int UserId { get; set; }
        public EnrollmentType EnrollmentType { get; set; }
        public int? LectureId { get; set; }
        public int? CourseId { get; set; }
        public int? SpecializationId { get; set; }
        public decimal Amount { get; set; }
        public string InvoicePdfUrl { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedByAdminId { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto User { get; set; }
        public UserDto? ApprovedByAdmin { get; set; }
        public LectureDto? Lecture { get; set; }
        public CourseDto? Course { get; set; }
        public SpecializationDto? Specialization { get; set; }
    }

    public class CreatePaymentRequestDto
    {
        public EnrollmentType EnrollmentType { get; set; }
        public int? LectureId { get; set; }
        public int? CourseId { get; set; }
        public int? SpecializationId { get; set; }
        public IFormFile InvoicePdf { get; set; }
        public int? UserId { get; set; }
    }
    public class CreatePaymentRequestDtos
    {
        public EnrollmentType EnrollmentType { get; set; }
        public int? LectureId { get; set; }
        public int? CourseId { get; set; }
        public int? SpecializationId { get; set; }
        public IBrowserFile InvoicePdf { get; set; }
    }

    public class UpdatePaymentRequestDto
    {
        public PaymentStatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }

    // In your DTOs/PaymentDtos.cs or create DashboardDtos.cs

    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalSpecializations { get; set; }
        public int TotalCourses { get; set; }
        public int TotalLectures { get; set; }
        public int TotalEnrollments { get; set; }
        public int ActiveEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public int PendingPayments { get; set; }
        public int ApprovedPayments { get; set; }
        public int RejectedPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal DoctorsBalance { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<PopularContentDto> PopularContent { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
    }

    public class PopularContentDto
    {
        public string ContentType { get; set; }
        public int ContentId { get; set; }
        public string Title { get; set; }
        public int EnrollmentCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageRating { get; set; }
    }
}

// DTOs/ReviewDtos.cs
namespace Medlearn.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public EnrollmentType ReviewType { get; set; }
        public int? LectureId { get; set; }
        public int? CourseId { get; set; }
        public int? SpecializationId { get; set; }
        public int EnrollmentId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto User { get; set; }
        public LectureDto? Lecture { get; set; }
        public CourseDto? Course { get; set; }
        public SpecializationDto? Specialization { get; set; }
    }

    public class CreateReviewDto
    {
        public int EnrollmentId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}

// DTOs/NotificationDtos.cs
namespace Medlearn.DTOs
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ThumbnailUrl { get; set; } // Add this
        public string? ImageAltText { get; set; } // Add this (optional)
        public int CreatedByAdminId { get; set; }
        public bool IsImportant { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool ForAllUsers { get; set; }
        public bool ForDoctorsOnly { get; set; }
        public bool ForRegularUsersOnly { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto CreatedByAdmin { get; set; }
    }

    public class CreateAnnouncementDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public IFormFile? ThumbnailImage { get; set; } // Add this
        public string? ImageAltText { get; set; } // Add this (optional)
        public bool IsImportant { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool ForAllUsers { get; set; }
        public bool ForDoctorsOnly { get; set; }
        public bool ForRegularUsersOnly { get; set; }
    }

    public class UpdateAnnouncementDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public IFormFile? ThumbnailImage { get; set; } // Add this
        public string? ImageAltText { get; set; } // Add this (optional)
        public bool? IsImportant { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool? ForAllUsers { get; set; }
        public bool? ForDoctorsOnly { get; set; }
        public bool? ForRegularUsersOnly { get; set; }
        public bool RemoveThumbnail { get; set; }
    }

    // Also create a DTO without IFormFile for when using IBrowserFile (Blazor)
    public class CreateAnnouncementDtos
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public IBrowserFile? ThumbnailImage { get; set; }
        public string? ImageAltText { get; set; }
        public bool IsImportant { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool ForAllUsers { get; set; }
        public bool ForDoctorsOnly { get; set; }
        public bool ForRegularUsersOnly { get; set; }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentDate { get; set; }
        public string? ActionUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string? ActionUrl { get; set; }
    }

    // Add to DTOs/PaymentDtos.cs or create new DTOs/WithdrawalDtos.cs
        public class WithdrawalRequestDto
        {
            public int Id { get; set; }
            public int DoctorId { get; set; }
            public decimal Amount { get; set; }
            public string PaymentMethod { get; set; }
            public string AccountDetails { get; set; }
            public WithdrawalStatus Status { get; set; }
            public DateTime RequestDate { get; set; }
            public DateTime? ProcessedDate { get; set; }
            public int? ProcessedByAdminId { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedAt { get; set; }
            public UserDto Doctor { get; set; }
            public UserDto? ProcessedByAdmin { get; set; }
        }

        public class CreateWithdrawalRequestDto
        {
            public decimal Amount { get; set; }
            public string PaymentMethod { get; set; }
            public string AccountDetails { get; set; }
        }

        public class ProcessWithdrawalDto
        {
            public WithdrawalStatus Status { get; set; }
            public string? Notes { get; set; }
        }

      
        public class DoctorBalanceDto
        {
            public decimal AvailableBalance { get; set; }
            public decimal PendingWithdrawals { get; set; }
            public decimal TotalEarned { get; set; }
            public List<WithdrawalRequestDto> RecentWithdrawals { get; set; } = new();
            public List<MonthlyEarningDto> MonthlyEarnings { get; set; } = new();
        }

        public class MonthlyEarningDto
        {
            public string Month { get; set; }
            public decimal Earnings { get; set; }
            public int Enrollments { get; set; }
        }
    // In your DTOs/UserDtos.cs file, add:
    public class AdminResetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public class SearchResultsDto
    {
        public string Query { get; set; } = string.Empty;
        public string Type { get; set; } = "all";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public List<SpecializationDto> Specializations { get; set; } = new();
        public List<CourseDto> Courses { get; set; } = new();
        public List<LectureDto> Lectures { get; set; } = new();
    }

    public class QuickSearchResultsDto
    {
        public List<SpecializationDto> Specializations { get; set; } = new();
        public List<CourseDto> Courses { get; set; } = new();
        public List<LectureDto> Lectures { get; set; } = new();
    }

    // You might want to add these properties to your existing DTOs for better search display
    public class SearchLectureDto : LectureDto
    {
        public string CourseTitle { get; set; } = string.Empty;
        public string SpecializationTitle { get; set; } = string.Empty;
    }

    public class SearchCourseDto : CourseDto
    {
        public string SpecializationTitle { get; set; } = string.Empty;
    }
    public class StudentStatsDto
    {
        public int TotalEnrollments { get; set; }
        public int InProgressEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public decimal AverageProgress { get; set; }
    }
    public class StudentStats
    {
        public int TotalEnrollments { get; set; }
        public int InProgressEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }
        public decimal AverageProgress { get; set; }
    }

    // DTOs/MessageDto.cs
   
        public class MessageDto
        {
            public int Id { get; set; }
            public int SenderId { get; set; }
            public int RecipientId { get; set; }
            public string? Subject { get; set; }
            public string? Content { get; set; }
            public bool IsRead { get; set; }
            public bool IsUrgent { get; set; }
            public bool RequestReadReceipt { get; set; }
            public bool AllowReplies { get; set; }
            public DateTime SentDate { get; set; }
            public DateTime? ReadDate { get; set; }
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }

            // Navigation properties
            public UserDto? Sender { get; set; }
            public UserDto? Recipient { get; set; }
            public List<MessageAttachmentDto>? Attachments { get; set; }
        public List<MessageDto> Replies { get; set; }
        public object ParentMessageId { get; set; }
        public bool IsFromPublic { get; set; }
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderPhone { get; set; }
    }

        public class MessageAttachmentDto
        {
            public int Id { get; set; }
            public int MessageId { get; set; }
            public string? FileName { get; set; }
            public long FileSize { get; set; }
            public string? ContentType { get; set; }
            public byte[]? FileData { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class SendMessageDto
        {
            public int RecipientId { get; set; }
            public string? Subject { get; set; }
            public string? Content { get; set; }
            public bool IsUrgent { get; set; }
            public bool RequestReadReceipt { get; set; }
            public bool AllowReplies { get; set; }
            public List<MessageAttachmentDto>? Attachments { get; set; }
        public bool IsFromPublic { get; set; }
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderPhone { get; set; }

    }

   
        public class ContactMessageDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public string Subject { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public bool SubscribeNewsletter { get; set; }
            public bool IsAuthenticatedUser { get; set; }
            public int? UserId { get; set; }
            public string? UserName { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsRead { get; set; }
            public DateTime? ReadAt { get; set; }
            public bool IsReplied { get; set; }
            public DateTime? RepliedAt { get; set; }
            public string? AdminReply { get; set; }
            public string? RepliedBy { get; set; }
            public string Status { get; set; } = "New";
            public string? IPAddress { get; set; }
            public string? UserAgent { get; set; }
        }

        public class CreateContactMessageDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public string Subject { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public bool SubscribeNewsletter { get; set; }
            public bool IsAuthenticatedUser { get; set; }
            public int? UserId { get; set; }
            public string? IPAddress { get; set; }
            public string? UserAgent { get; set; }
        }

        public class ContactReplyDto
        {
            public string Reply { get; set; } = string.Empty;
        }
    public class UserWithLastMessageDto : UserDto
    {
        public DateTime? LastMessageDate { get; set; }
    }
    public class NewsletterSubscriberDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public DateTime SubscribedAt { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
        public bool IsActive { get; set; }
    }
}

    


