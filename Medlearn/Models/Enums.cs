namespace Medlearn.Model
{
    public enum UserType
    {
        User = 0,
        Doctor = 1,
        Admin = 2
    }

   public enum MaterialType
{
    Document = 7,
    Video = 2,
    Audio = 3,
    Image = 4,
    Text = 5,
    Presentation = 6,
    PDF = 1,
    Other = 8
}
    public enum WithdrawalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Completed = 3
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3
    }

    public enum EnrollmentType
    {
        Lecture = 0,
        Course = 1,
        Specialization = 2
    }
}
