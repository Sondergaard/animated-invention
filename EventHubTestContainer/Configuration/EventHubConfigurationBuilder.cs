using System.Diagnostics.CodeAnalysis;

namespace EventHubTestContainer.Configuration;

public sealed class EventHubConfigurationBuilder
{
    private readonly List<Action<EntityConfigurationBuilder>> _entityConfigurations = new();


    public EventHubConfigurationBuilder AddEntity(Action<EntityConfigurationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));
        _entityConfigurations.Add(configure);

        return this;
    }

    public EventHubConfiguration Build()
    {
        var entities = _entityConfigurations.Select(configure =>
        {
            var entityConfiguration = new EntityConfigurationBuilder();
            configure(entityConfiguration);
            return entityConfiguration.Build();
        }).ToArray();

        var namespaceConfiguration = new NamespaceConfiguration(entities);

        return new EventHubConfiguration(namespaceConfiguration);
    }
}