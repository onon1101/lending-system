namespace LendingSystem.SharedKernel.Infrastructure.Persistence;

public static class ApplicationGeneratedKey
{
    private const long EpochMilliseconds = 1_672_531_200_000; // 2023-01-01T00:00:00Z
    private const int NodeIdBits = 10;
    private const int SequenceBits = 12;
    private const int MaxSequence = (1 << SequenceBits) - 1;

    private static readonly object Sync = new();
    private static readonly long NodeId = Environment.MachineName.GetHashCode(StringComparison.Ordinal) & ((1 << NodeIdBits) - 1);

    private static long lastTimestamp = -1;
    private static int sequence;

    public static long NewId()
    {
        lock (Sync)
        {
            var timestamp = CurrentMilliseconds();
            if (timestamp < lastTimestamp)
            {
                timestamp = lastTimestamp;
            }

            if (timestamp == lastTimestamp)
            {
                sequence = (sequence + 1) & MaxSequence;
                if (sequence == 0)
                {
                    timestamp = WaitNextMillisecond(lastTimestamp);
                }
            }
            else
            {
                sequence = 0;
            }

            lastTimestamp = timestamp;

            return ((timestamp - EpochMilliseconds) << (NodeIdBits + SequenceBits))
                | (NodeId << SequenceBits)
                | (uint)sequence;
        }
    }

    private static long WaitNextMillisecond(long timestamp)
    {
        var next = CurrentMilliseconds();
        while (next <= timestamp)
        {
            Thread.SpinWait(20);
            next = CurrentMilliseconds();
        }

        return next;
    }

    private static long CurrentMilliseconds() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
