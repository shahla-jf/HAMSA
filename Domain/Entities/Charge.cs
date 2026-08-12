using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

// شارژ ماهانه واحد
public class Charge
{
    public Guid Id { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid BuildingId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal Amount { get; private set; }
    public decimal PenaltyAmount { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // روابط
    public Unit Unit { get; private set; } = null!;
    public Building Building { get; private set; } = null!;

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    private readonly List<Transaction> _transactions = new();

    private Charge() { }

    public static Charge Create(Guid unitId, Guid buildingId, int year, int month, decimal amount, DateTime dueDate)
    {
        return new Charge
        {
            Id = Guid.NewGuid(),
            UnitId = unitId,
            BuildingId = buildingId,
            Year = year,
            Month = month,
            Amount = amount,
            IsPaid = false,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsPaid()
    {
        IsPaid = true;
        PaidAt = DateTime.UtcNow;
    }

    public void ApplyPenalty(decimal penaltyPercent)
    {
        PenaltyAmount = Amount * penaltyPercent / 100;
    }
}

// تراکنش مالی
public class Transaction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ChargeId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string? TrackingCode { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = null!;
    public Charge Charge { get; private set; } = null!;

    private Transaction() { }

    public static Transaction Create(Guid userId, Guid chargeId, decimal amount)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ChargeId = chargeId,
            Amount = amount,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsPaid(string trackingCode)
    {
        Status = TransactionStatus.Paid;
        TrackingCode = trackingCode;
    }

    public void MarkAsFailed()
    {
        Status = TransactionStatus.Failed;
    }
}

// هزینه‌های ساختمان (که مدیر ثبت می‌کند)
public class BuildingExpense
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid CreatedByUserId { get; private set; } // مدیر ثبت کننده
    public ExpenseCategory Category { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    
    // فیلدهای قبلی برای حفظ ساختار دیتابیس
    public int Year { get; private set; }
    public int Month { get; private set; }
    public bool IsPaid { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Building Building { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    private BuildingExpense() { }

    public static BuildingExpense Create(
        Guid buildingId, Guid createdByUserId, ExpenseCategory category, string title, decimal amount)
    {
        var now = DateTime.UtcNow;
        return new BuildingExpense
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            CreatedByUserId = createdByUserId,
            Category = category,
            Title = title,
            Amount = amount,
            Year = now.Year,
            Month = now.Month,
            IsPaid = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(ExpenseCategory category, string title, decimal amount)
    {
        Category = category;
        Title = title;
        Amount = amount;
        UpdatedAt = DateTime.UtcNow;
    }
}