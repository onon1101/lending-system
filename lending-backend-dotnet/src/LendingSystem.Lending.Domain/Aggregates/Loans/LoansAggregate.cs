using LendingSystem.Lending.Domain.Aggregates.Item;
using LendingSystem.SharedKernel.Domain.Abstractions;
using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Aggregates.Loans;



public sealed class LoansAggregate : Entity, IAggregateRoot
{
    private readonly List<Loan> _loans;
    private readonly List<LoanMedia> _media;

    private LoansAggregate(
        long borrowerDetailId,
        long? borrowerUserId,
        string borrowerName,
        IEnumerable<Loan> loans,
        IEnumerable<LoanMedia>? media)
    {
        BorrowerDetailId = borrowerDetailId;
        BorrowerUserId = borrowerUserId;
        BorrowerName = borrowerName;
        _loans = loans.ToList();
        _media = media?.ToList() ?? [];
    }

    public long BorrowerDetailId { get; }
    public long? BorrowerUserId { get; }
    public string BorrowerName { get; }
    public IReadOnlyCollection<Loan> LoanEntries => _loans.AsReadOnly();
    public IReadOnlyCollection<LoanMedia> Media => _media.AsReadOnly();

    /// <summary>
    /// 從資料復原資料
    /// </summary>
    /// <param name="borrowerDetailId"></param>
    /// <param name="borrowerUserId"></param>
    /// <param name="borrowerName"></param>
    /// <param name="loans"></param>
    /// <param name="media"></param>
    /// <returns></returns>
    public static LoansAggregate Rehydrate(
        long borrowerDetailId,
        long? borrowerUserId,
        string borrowerName,
        IEnumerable<Loan> loans,
        IEnumerable<LoanMedia>? media = null) =>
        new(borrowerDetailId, borrowerUserId, borrowerName, loans, media);

    public static LoansAggregate Create(
        long borrowerDetailId,
        long? borrowerUserId,
        string borrowerName,
        IEnumerable<Loan> loans)
    {
        var aggregate = new LoansAggregate(borrowerDetailId, borrowerUserId, borrowerName, loans, null);
        foreach (var loan in aggregate.LoanEntries.Where(x => x.OrderId == 0))
        {
            aggregate.AddDomainEvent(new LoanCreatedDomainEvent(borrowerDetailId, loan));
        }

        return aggregate;
    }

    public static LoansAggregate Create(
        long borrowerDetailId,
        long itemOwnerId,
        string itemOwnerName,
        long borrowerUserId,
        string borrowerName,
        long itemId,
        string itemName,
        DateOnly startDate,
        DateOnly endDate,
        bool isBorrowingRequest)
    {
        if (!isBorrowingRequest)
        {
            throw new InvalidOperationException("Loan request creation requires request mode.");
        }

        if (startDate >= endDate)
        {
            throw new InvalidOperationException("Loan start date must be earlier than end date.");
        }

        var loan = Loan.CreateRequest(itemId, startDate, endDate);
        var aggregate = new LoansAggregate(borrowerDetailId, borrowerUserId, borrowerName, [loan], null);
        aggregate.AddDomainEvent(new LoanRequestCreatedDomainEvent(
            borrowerDetailId,
            itemOwnerId,
            itemOwnerName,
            borrowerUserId,
            borrowerName,
            itemName,
            loan));

        return aggregate;
    }

    public void AddMedia(LoanMedia media)
    {
        if (media.OrderId > 0 && _loans.All(x => x.OrderId != media.OrderId))
        {
            throw new InvalidOperationException("Media does not belong to any loan in this aggregate.");
        }

        _media.Add(media);
        AddDomainEvent(new LoanMediaAddedDomainEvent(media));
    }

    public static Result<LoansAggregate> RequestBorrowing(
        long borrowerDetailId,
        long itemOwnerId,
        string itemOwnerName,
        long borrowerUserId,
        string borrowerName,
        long itemId,
        string itemName,
        string itemStatus,
        LoanPeriod period)
    {
        if (borrowerUserId == itemOwnerId)
        {
            return Result<LoansAggregate>.Failure(LoanDomainError.CannotBorrowOwnItem());
        }

        if (itemStatus != ItemStatuses.Available)
        {
            return Result<LoansAggregate>.Failure(LoanDomainError.ItemMustBeAvailable(itemId));
        }

        var loan = Loan.CreateRequest(itemId, period);

        var aggregate = new LoansAggregate(
            borrowerDetailId,
            borrowerUserId,
            borrowerName,
            [loan],
            null);

        aggregate.AddDomainEvent(new LoanRequestCreatedDomainEvent(
            borrowerDetailId,
            itemOwnerId,
            itemOwnerName,
            borrowerUserId,
            borrowerName,
            itemName,
            loan));

        return Result<LoansAggregate>.Success(aggregate);
    }
}


public sealed class LoanCreatedDomainEvent(long borrowerDetailId, Loan loan) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public long BorrowerDetailId { get; } = borrowerDetailId;
    public Loan Loan { get; } = loan;
    public UserLoan? CreatedLoan { get; set; }
}

public sealed class LoanRequestCreatedDomainEvent(
    long borrowerDetailId,
    long itemOwnerId,
    string itemOwnerName,
    long borrowerUserId,
    string borrowerName,
    string itemName,
    Loan loan) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public long BorrowerDetailId { get; } = borrowerDetailId;
    public long ItemOwnerId { get; } = itemOwnerId;
    public string ItemOwnerName { get; } = itemOwnerName;
    public long BorrowerUserId { get; } = borrowerUserId;
    public string BorrowerName { get; } = borrowerName;
    public string ItemName { get; } = itemName;
    public Loan Loan { get; } = loan;
    public UserLoan? CreatedLoan { get; set; }
}

public sealed class LoanMediaAddedDomainEvent(LoanMedia media) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public LoanMedia Media { get; } = media;
    public LoanMedia? CreatedMedia { get; set; }
}
