namespace LendingSystem.SharedKernel.Domain.Common;

public abstract class SingleValueObject<TValue> : ValueObject<TValue>
where TValue : notnull
{
    protected SingleValueObject(TValue value)
    {
        Value = value;
    }
    
    public TValue Value{ get; }

    protected override IEnumerable<TValue?> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string? ToString() => Value?.ToString();
}