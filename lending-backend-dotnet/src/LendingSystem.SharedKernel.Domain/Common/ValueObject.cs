namespace LendingSystem.SharedKernel.Domain.Common;

public abstract class ValueObject<TValue>
{
    protected abstract IEnumerable<TValue?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject<TValue> other ||
            obj.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                1,
                (current, component) => HashCode.Combine(current, component));
    }

    public static bool operator ==(
        ValueObject<TValue>? left,
        ValueObject<TValue>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        ValueObject<TValue>? left,
        ValueObject<TValue>? right)
    {
        return !Equals(left, right);
    }
}
