namespace EventHubTestContainer.Configuration;

/// <summary>
///     A builder class for creating an EntityConfiguration instance.
/// </summary>
/// <remarks>
///     This class provides methods to set the name and partition count of an entity,
///     and to add consumer groups to the entity. It ensures that the entity name is not null or empty,
///     the partition count is not less than the minimum allowed,
///     and the number of consumer groups does not exceed the maximum allowed.
/// </remarks>
public sealed class EntityConfigurationBuilder
{
    private const int DefaultPartitionCount = 2;
    public const int MaxConsumerGroups = 20;
    private static int _instanceCount;
    private readonly List<ConsumerGroup> _consumerGroups = new();
    private string _entityName;
    private int _partitionCount;

    internal EntityConfigurationBuilder()
    {
        var instance = Interlocked.Increment(ref _instanceCount);
        _entityName = $"entity-{instance:D3}";
        _partitionCount = DefaultPartitionCount;
    }

    public EntityConfigurationBuilder WithName(string entityName)
    {
        _entityName = entityName;
        return this;
    }

    public EntityConfigurationBuilder WithPartitionCount(int partitionCount)
    {
        _partitionCount = partitionCount;
        return this;
    }

    public EntityConfigurationBuilder AddConsumerGroup(ConsumerGroup consumerGroup)
    {
        _consumerGroups.Add(consumerGroup);
        return this;
    }

    public EntityConfigurationBuilder AddConsumerGroup(string consumerGroupName)
    {
        return AddConsumerGroup(new ConsumerGroup(consumerGroupName));
    }

    public EntityConfiguration Build()
    {
        return new EntityConfiguration(_entityName, _partitionCount, _consumerGroups.ToArray());
    }
}