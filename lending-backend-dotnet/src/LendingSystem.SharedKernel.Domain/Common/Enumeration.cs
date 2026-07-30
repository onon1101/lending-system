public abstract class Enumeration<TKey, TValue>
    : IComparable,
      IComparable<Enumeration<TKey, TValue>>,
      IEquatable<Enumeration<TKey, TValue>>
{
    public TKey Key { get; }

    public TValue Value { get; }

    protected Enumeration(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }

    public bool Equals(Enumeration<TKey, TValue>? other)
    {
        if (other is null)
            return false;

        return GetType() == other.GetType()
            && EqualityComparer<TKey>.Default.Equals(Key, other.Key);
    }

    public override bool Equals(object? obj)
    {
        return obj is Enumeration<TKey, TValue> other
            && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Key);
    }

    public int CompareTo(Enumeration<TKey, TValue>? other)
    {
        if (other is null)
            return 1;

        if (GetType() != other.GetType())
        {
            throw new ArgumentException(
                "Cannot compare different enumeration types.",
                nameof(other));
        }

        return Comparer<TKey>.Default.Compare(Key, other.Key);
    }

    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        if (obj is not Enumeration<TKey, TValue> other)
        {
            throw new ArgumentException(
                $"Object must be of type {GetType().Name}.",
                nameof(obj));
        }

        return CompareTo(other);
    }

    public static bool operator ==(
        Enumeration<TKey, TValue>? left,
        Enumeration<TKey, TValue>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        Enumeration<TKey, TValue>? left,
        Enumeration<TKey, TValue>? right)
    {
        return !Equals(left, right);
    }

    public static bool operator <(
        Enumeration<TKey, TValue> left,
        Enumeration<TKey, TValue> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(
        Enumeration<TKey, TValue> left,
        Enumeration<TKey, TValue> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.CompareTo(right) > 0;
    }

    public override string? ToString()
    {
        return Value?.ToString();
    }
}