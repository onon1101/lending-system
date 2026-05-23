using LendingSystem.SharedKernel.Domain.Abstractions;
using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Aggregate.Loans;

public static class LoanStatuses
{
    /// <summary>
    /// 已經送出請求給
    /// </summary>
    public const string Requested = "Requested";
    
    /// <summary>
    /// 已同意
    /// </summary>
    public const string Approved = "Approved";

    /// <summary>
    /// 已經由借閱者拿取，正在使用
    /// </summary>
    public const string OnLoan = "On Loan";

    /// <summary>
    /// 已歸還
    /// </summary>
    public const string Returned = "Returned";
}

public sealed record LoanItemDetail(
    int ObjectDetailId,
    int ObjectId,
    string ObjectName,
    string DetailStatus,
    DateOnly? ActualReturnDate);

public sealed record UserLoan(
    int OrderId,
    int UserId,
    DateOnly OrderStartDate,
    DateOnly OrderEndDate,
    string OrderStatus,
    IReadOnlyCollection<LoanItemDetail> Items);

public sealed record LoanRecord(
    int? OrderId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Name,
    string? Status);

public sealed record LoanRequestRecord(
    int OrderId,
    string ItemName,
    string BorrowerName,
    string BorrowerUsername,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);

public static class LoanDomainError
{
    /// <summary>
    /// 開始時間不得大於結束時間
    /// </summary>
    public static DomainErrors StartDateMustBeEarlierThanEndDate() =>
        new("LOAN_START_DATE_MUST_BE_EARLIER_THAN_END_DATE",
            "start_date must be earlier than end_date",
            "Loan start date must be earlier than end date");

    /// <summary>
    /// 查無借閱紀錄
    /// </summary>
    public static DomainErrors LoanRecordNotFound(int orderId) =>
        new("LOAN_ISSUE",
            $"Loan record {orderId} was not found.",
            "Loan record was not found.");

    /// <summary>
    /// 物品不屬於使用者
    /// </summary>
    public static DomainErrors ItemDoesNotBelongToOwner(int itemId, int ownerId) =>
        new("ITEM_ISSUE",
            $"Item {itemId} doesn't belong to owner {ownerId}",
            "Some item issue");
}

public sealed class LoansAggregate : Entity, IAggregateRoot
{
    private readonly List<Loan> _loans;
    private readonly List<LoanMedia> _media;

    private LoansAggregate(
        int borrowerDetailId,
        int? borrowerUserId,
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

    public int BorrowerDetailId { get; }
    public int? BorrowerUserId { get; }
    public string BorrowerName { get; }
    public IReadOnlyCollection<Loan> LoanEntries => _loans.AsReadOnly();
    public IReadOnlyCollection<LoanMedia> Media => _media.AsReadOnly();

    public static LoansAggregate Rehydrate(
        int borrowerDetailId,
        int? borrowerUserId,
        string borrowerName,
        IEnumerable<Loan> loans,
        IEnumerable<LoanMedia>? media = null) =>
        new(borrowerDetailId, borrowerUserId, borrowerName, loans, media);

    public static LoansAggregate Create(
        int borrowerDetailId,
        int? borrowerUserId,
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
        int borrowerDetailId,
        int itemOwnerId,
        string itemOwnerName,
        int borrowerUserId,
        string borrowerName,
        int itemId,
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
}

public sealed class LoanCreatedDomainEvent(int borrowerDetailId, Loan loan) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int BorrowerDetailId { get; } = borrowerDetailId;
    public Loan Loan { get; } = loan;
    public UserLoan? CreatedLoan { get; set; }
}

public sealed class LoanRequestCreatedDomainEvent(
    int borrowerDetailId,
    int itemOwnerId,
    string itemOwnerName,
    int borrowerUserId,
    string borrowerName,
    string itemName,
    Loan loan) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int BorrowerDetailId { get; } = borrowerDetailId;
    public int ItemOwnerId { get; } = itemOwnerId;
    public string ItemOwnerName { get; } = itemOwnerName;
    public int BorrowerUserId { get; } = borrowerUserId;
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
