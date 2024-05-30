namespace EventHubTestContainer.Configuration;

public class ConsumerGroup
{
    public const string ReservedConsumerGroupName = "$default";

    public ConsumerGroup(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        if (name.Equals(ReservedConsumerGroupName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Consumer group name '{ReservedConsumerGroupName}' is reserved.");

        Name = name;
    }

    public string Name { get; }

    public bool Equals(ConsumerGroup other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ConsumerGroup)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}