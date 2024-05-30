namespace EventHubTestContainer.Configuration;

public class NamespaceConfiguration
{
    public const string EmulatorNamespaceName = "emulatorNs1"; // Hardcoded in the emulator
    public const int MaxNoOfEntities = 10; // Maximum number of entities allowed in the emulator

    public NamespaceConfiguration(EntityConfiguration[] entityConfigurations)
    {
        ArgumentNullException.ThrowIfNull(entityConfigurations, nameof(entityConfigurations));
        if (entityConfigurations.Length == 0) throw new InvalidOperationException("At least one entity must be added.");
        if (entityConfigurations.Length > MaxNoOfEntities)
            throw new InvalidOperationException($"A maximum of {MaxNoOfEntities} entities can be added.");
        if (entityConfigurations.Any(e => e.ConsumerGroups.Length == 0))
            throw new InvalidOperationException("At least one consumer group must be added to each entity.");

        EntityConfigurations = entityConfigurations;
    }

    public static string NamespaceName => EmulatorNamespaceName;
    public EntityConfiguration[] EntityConfigurations { get; }
}