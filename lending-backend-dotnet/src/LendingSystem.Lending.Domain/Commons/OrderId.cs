using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Commons;

public sealed class OrderId : ValueObject<long>
{
    private OrderId(long value) { Value = value; }
    public long Value { get; }
    public static Result<OrderId> Create(long value)
    {
        var result = new OrderId(value);
        return Result<OrderId>.Success(result);
    }
    protected override IEnumerable<long> GetEqualityComponents()
    {
        throw new NotImplementedException();
    }
}
