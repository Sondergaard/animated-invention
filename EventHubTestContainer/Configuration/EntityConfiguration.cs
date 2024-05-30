namespace EventHubTestContainer.Configuration;

public class EntityConfiguration
{
    private const int MinPartitionCount = 1;
    public const int MaxConsumerGroups = 20;

    public EntityConfiguration(string entityName, int partitionCount, ConsumerGroup[] consumerGroups)
    {
        ArgumentException.ThrowIfNullOrEmpty(entityName, nameof(entityName));
        ArgumentOutOfRangeException.ThrowIfLessThan(partitionCount, MinPartitionCount, nameof(partitionCount));
        if (consumerGroups.Length == 0)
            throw new InvalidOperationException("At least one consumer group must be added.");
        if (consumerGroups.Length > MaxConsumerGroups)
            throw new InvalidOperationException($"A maximum of {MaxConsumerGroups} consumer groups can be added.");
        if (ConsumerGroupsContainsDuplicate(consumerGroups))
            throw new InvalidOperationException("Consumer group names must be unique.");

        EntityName = entityName;
        PartitionCount = partitionCount;
        ConsumerGroups = consumerGroups;
    }

    public ConsumerGroup[] ConsumerGroups { get; }

    public int PartitionCount { get; }

    public string EntityName { get; }

    private static bool ConsumerGroupsContainsDuplicate(ConsumerGroup[] consumerGroups)
    {
        return consumerGroups.GroupBy(cg => cg.Name).Any(g => g.Count() > 1);
    }
}