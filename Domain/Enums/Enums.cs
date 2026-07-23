namespace HAMSA.Domain.Enums;

public enum UserRole
{
    Manager,       // مدیر ساختمان
    Owner,         // مالک
    Tenant         // مستاجر
}

public enum AnnouncementPriority
{
    Normal,
    Important,
    Urgent        // فوری
}

public enum AnnouncementSource
{
    System,
    Manager
}

public enum RepairPriority
{
    Low,
    Medium,
    High,
    Critical      // بحرانی
}

public enum RepairStatus
{
    Registered,       // ثبت‌شده
    UnderReview,      // در حال بررسی
    InProgress,       // در دست اقدام
    Done              // انجام‌شده
}

public enum PollAudience
{
    All,
    Owners,
    Tenants
}

public enum FacilityType
{
    Gym,            // سالن ورزشی
    Pool,           // استخر
    MeetingHall,    // سالن اجتماعات
    RoofGarden      // روف‌گاردن
}

public enum ExpenseCategory
{
    Elevator,       // آسانسور
    Cleaning,       // نظافت
    Electricity,    // برق مشاعات
    Water,          // آب مشاعات
    Repairs,        // تعمیرات
    Other
}

public enum TransactionStatus
{
    Paid,
    Pending,
    Failed
}

public enum ListingType
{
    Sale,           // فروش
    Loan,           // قرض
    FreeGift        // اهدای رایگان
}

public enum LocalServiceCategory
{
    Repairs,        // تعمیرات
    Cleaning,       // نظافت
    Education,      // آموزش
    Transportation, // حمل و نقل
    Health,         // سلامت
    Beauty,         // زیبایی
    Legal,          // حقوقی
    Renovation      // بازسازی
}

public enum EventCategory
{
    Cooking,        // آشپزی
    Educational,    // آموزشی
    Beauty,         // زیبایی
    Technical,      // فنی
    Sports,         // ورزشی
    BookReading     // کتابخوانی
}

public enum Gender
{
    Male,
    Female,
    Other
}

public enum GroupChallengeStatus
{
    RegistrationOpen,   // شنبه تا دوشنبه - باز برای ثبت‌نام
    ChallengeActive,    // دوشنبه تا جمعه - چالش فعال
    Completed           // پایان یافته
}
