using LendingSystem.Lending.Domain.Aggregates.Item;
using LendingSystem.SharedKernel.Domain.Abstractions;
using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Aggregates.Loans;

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
    long ObjectDetailId,
    long ObjectId,
    string ObjectName,
    string DetailStatus,
    DateOnly? ActualReturnDate);

public sealed record UserLoan(
    long OrderId,
    long UserId,
    DateOnly OrderStartDate,
    DateOnly OrderEndDate,
    string OrderStatus,
    IReadOnlyCollection<LoanItemDetail> Items);

public sealed record LoanRecord(
    long? OrderId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Name,
    string? Status);

public sealed record LoanRequestRecord(
    long OrderId,
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

    public static DomainErrors DurationDaysMustBePositive() =>
        new(
            "LOAN_DURATION_DAYS_MUST_BE_POSITIVE",
            "duration_days must be greater than 0",
            "Loan duration days must be greater than 0");

    public static DomainErrors CannotBorrowOwnItem() =>
        new(
            "LOAN_CANNOT_BORROW_OWN_ITEM",
            "Borrower cannot borrow their own item",
            "Borrower cannot borrow their own item");

    public static DomainErrors ItemMustBeAvailable(long itemId) =>
        new(
            "LOAN_ITEM_MUST_BE_AVAILABLE",
            $"Item {itemId} must be available before creating a loan request",
            "Item must be available before creating a loan request");

    /// <summary>
    /// 查無借閱紀錄
    /// </summary>
    public static DomainErrors LoanRecordNotFound(long orderId) =>
        new("LOAN_ISSUE",
            $"Loan record {orderId} was not found.",
            "Loan record was not found.");

    /// <summary>
    /// 物品不屬於使用者
    /// </summary>
    public static DomainErrors ItemDoesNotBelongToOwner(long itemId, long ownerId) =>
        new("ITEM_ISSUE",
            $"Item {itemId} doesn't belong to owner {ownerId}",
            "Some item issue");
}

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

    public static LoansAggregate RequestBorrowing(
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
        CheckRule(new CannotBorrowOwnItemRule(borrowerUserId, itemOwnerId));
        CheckRule(new ItemMustBeAvailableRule(itemId, itemStatus));

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

        return aggregate;
    }
}

public sealed class CannotBorrowOwnItemRule(long borrowerUserId, long itemOwnerId) : IBusinessRule
{
    public long BorrowerUserId { get; } = borrowerUserId;
    public long ItemOwnerId { get; } = itemOwnerId;

    public bool IsBroken() => BorrowerUserId == ItemOwnerId;

    public string Message => "Borrower cannot borrow their own item.";
}

public sealed class ItemMustBeAvailableRule(long itemId, string itemStatus) : IBusinessRule
{
    public long ItemId { get; } = itemId;
    public string ItemStatus { get; } = itemStatus;

    public bool IsBroken() => ItemStatus != ItemStatuses.Available;

    public string Message => $"Item {ItemId} must be available before creating a loan request.";
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
