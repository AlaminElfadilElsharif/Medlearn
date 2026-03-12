namespace Medlearn.Model
{
    // Base entity with common properties
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // User entity
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserType UserType { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }

        // Navigation properties
        public virtual Profile Profile { get; set; } = null!;

        // Doctor's content
        public virtual ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();

        // User's enrollments and activity
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // Admin content
        public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

        // Doctor's financials
        public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();

        // Messaging
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }

    // Profile entity for custom user profiles
    public class Profile : BaseEntity
    {
        public int UserId { get; set; }

        // User-specific fields
        public string? Title { get; set; }
        public string? Department { get; set; }
        public string? Institution { get; set; }
        public string? Expertise { get; set; }
        public string? LearningGoals { get; set; }
        public string? Interests { get; set; }
        public string? EducationBackground { get; set; }

        // Doctor-specific
        public decimal Balance { get; set; }
        public int TotalStudents { get; set; }
        public decimal RatingAverage { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;
    }

    // Specialization entity
    public class Specialization : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int EstimatedDurationHours { get; set; }
        public string? Prerequisites { get; set; }
        public int? DoctorId { get; set; }
        public bool IsFeatured { get; set; }

        // Navigation properties
        public virtual User? Doctor { get; set; }
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
    }

    // Course entity
    public class Course : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Order { get; set; }
        public int SpecializationId { get; set; }
        public int? DoctorId { get; set; }

        // Navigation properties
        public virtual Specialization Specialization { get; set; } = null!;
        public virtual User? Doctor { get; set; }
        public virtual ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
    }
    // Lecture entity
    public class Lecture : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public int CourseId { get; set; }
        public int? DoctorId { get; set; }
        public decimal Price { get; set; }
        public bool IsFreePreview { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        // Navigation properties
        public virtual Course Course { get; set; } = null!;
        public virtual User? Doctor { get; set; }
        public virtual ICollection<LearningMaterial> Materials { get; set; } = new List<LearningMaterial>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
    }
    // Learning material (video, file, etc.)
    public class LearningMaterial : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MaterialType Type { get; set; }
        public string ContentUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int DurationSeconds { get; set; }
        public long FileSize { get; set; }
        public int LectureId { get; set; }
        public int Order { get; set; }
        public bool IsDownloadable { get; set; }

        // Navigation property
        public virtual Lecture Lecture { get; set; } = null!;
    }

    // Enrollment entity
    public class Enrollment : BaseEntity
    {
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

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Lecture? Lecture { get; set; }
        public virtual Course? Course { get; set; }
        public virtual Specialization? Specialization { get; set; }
        public virtual ICollection<ProgressTracking> ProgressTrackings { get; set; } = new List<ProgressTracking>();
        public virtual Review? Review { get; set; }
    }
    // Progress tracking for each material
    public class ProgressTracking : BaseEntity
    {
        public int EnrollmentId { get; set; }
        public int MaterialId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int TimeSpentSeconds { get; set; }
        public int LastPositionSeconds { get; set; }

        // Navigation properties
        public virtual Enrollment Enrollment { get; set; } = null!;
        public virtual LearningMaterial Material { get; set; } = null!;
    }

    // Payment request system
    public class PaymentRequest : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public EnrollmentType EnrollmentType { get; set; }
        public int? LectureId { get; set; }
        public int? CourseId { get; set; }
        public int? SpecializationId { get; set; }
        public decimal Amount { get; set; }
        public string InvoicePdfUrl { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedByAdminId { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual User? ApprovedByAdmin { get; set; }
        public virtual Lecture? Lecture { get; set; }
        public virtual Course? Course { get; set; }
        public virtual Specialization? Specialization { get; set; }
    }
    // Review system
    public class Review : BaseEntity
    {
        public int UserId { get; set; }
        public EnrollmentType ReviewType { get; set; }
        public int? LectureId { get; set; }
        public int? CourseId { get; set; }
        public int? SpecializationId { get; set; }
        public int EnrollmentId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Enrollment Enrollment { get; set; } = null!;
        public virtual Lecture? Lecture { get; set; }
        public virtual Course? Course { get; set; }
        public virtual Specialization? Specialization { get; set; }
    }
    // Announcement system
    public class Announcement : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; } // Add this
        public string? ImageAltText { get; set; } // Add this
        public int CreatedByAdminId { get; set; }
        public bool IsImportant { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Target audience
        public bool ForAllUsers { get; set; }
        public bool ForDoctorsOnly { get; set; }
        public bool ForRegularUsersOnly { get; set; }

        // Navigation
        public virtual User CreatedByAdmin { get; set; } = null!;
    }

    // Notification system
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentDate { get; set; }
        public string? ActionUrl { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
    public class WithdrawalRequest : BaseEntity
    {
        public int DoctorId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string AccountDetails { get; set; } = string.Empty;
        public WithdrawalStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public int? ProcessedByAdminId { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual User Doctor { get; set; } = null!;
        public virtual User? ProcessedByAdmin { get; set; }
    }
    public class Message : BaseEntity
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string? Subject { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsUrgent { get; set; }
        public bool RequestReadReceipt { get; set; }
        public bool AllowReplies { get; set; }
        public int? ParentMessageId { get; set; }
        public DateTime SentDate { get; set; }
        public DateTime? ReadDate { get; set; }

        // Public message fields (for contact form)
        public bool IsFromPublic { get; set; }
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderPhone { get; set; }

        // Navigation properties
        public virtual User Sender { get; set; } = null!;
        public virtual User Recipient { get; set; } = null!;
        public virtual Message? ParentMessage { get; set; }
        public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
        public virtual ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
    }
    public class MessageAttachment : BaseEntity
    {
        public int MessageId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();

        // Navigation property
        public virtual Message Message { get; set; } = null!;
    }
    public class ContactMessage
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
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsReplied { get; set; }
        public DateTime? RepliedAt { get; set; }
        public string? AdminReply { get; set; }
        public string? RepliedBy { get; set; }
        public string Status { get; set; } = "New";

        // Navigation property
        public virtual User? User { get; set; }
    }
    public class NewsletterSubscriber : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public DateTime SubscribedAt { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
    }
    public class AdminAction : BaseEntity
    {
        public int AdminId { get; set; }
        public int? UserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public virtual User Admin { get; set; } = null!;
        public virtual User? User { get; set; }
    }
}
