namespace HAMSA.Domain.Entities;

// مبلغ شارژ تعیین‌شده توسط مدیر برای هر ماه (قبل از صدور شارژ برای واحدها)
public class MonthlyChargeRate
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsIssued { get; private set; }   // آیا شارژ برای واحدها صادر شده یا نه
    public DateTime CreatedAt { get; private set; }

    public Building Building { get; private set; } = null!;

    private MonthlyChargeRate() { }

    public static MonthlyChargeRate Create(Guid buildingId, int year, int month, decimal amount)
    {
        return new MonthlyChargeRate
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            Year = year,
            Month = month,
            Amount = amount,
            IsIssued = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateAmount(decimal amount)
    {
        if (IsIssued)
            throw new InvalidOperationException("شارژ این ماه قبلاً صادر شده و قابل ویرایش نیست");
        Amount = amount;
    }

    public void MarkAsIssued()
    {
        IsIssued = true;
    }
}
