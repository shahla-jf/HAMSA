using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HAMSA.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<BuildingMembership> BuildingMemberships => Set<BuildingMembership>();
    public DbSet<BuildingManagerHistory> BuildingManagerHistories => Set<BuildingManagerHistory>();

    public DbSet<Charge> Charges => Set<Charge>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<BuildingExpense> BuildingExpenses => Set<BuildingExpense>();

    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementRead> AnnouncementReads => Set<AnnouncementRead>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<RepairReport> RepairReports => Set<RepairReport>();
    public DbSet<RepairReportMedia> RepairReportMedias => Set<RepairReportMedia>();

    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<PollVote> PollVotes => Set<PollVote>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<GroupChallenge> GroupChallenges => Set<GroupChallenge>();
    public DbSet<GroupChallengeParticipant> GroupChallengeParticipants => Set<GroupChallengeParticipant>();

    public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();
    public DbSet<ChatGroupMember> ChatGroupMembers => Set<ChatGroupMember>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ResidentEvent> ResidentEvents => Set<ResidentEvent>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    public DbSet<LocalService> LocalServices => Set<LocalService>();
    public DbSet<LocalServiceRating> LocalServiceRatings => Set<LocalServiceRating>();

    public DbSet<GroupBuying> GroupBuyings => Set<GroupBuying>();
    public DbSet<GroupBuyingParticipant> GroupBuyingParticipants => Set<GroupBuyingParticipant>();
    
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<RevokedToken> RevokedTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}