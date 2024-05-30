using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using EventHubTestContainer.Configuration;
using Testcontainers.Azurite;

namespace EventHubTestContainer;

public sealed class EventHubComposer : IAsyncDisposable
{
    private static int _instanceCount;
    private readonly IContainer _azurite;
    private readonly IContainer _eventhub;
    private readonly INetwork _network;

    private EventHubComposer(byte[] configBytes)
    {
        var instance = Interlocked.Increment(ref _instanceCount);
        _network = new NetworkBuilder()
            .WithName($"eh-emulator-{instance:D3}").Build();

        _azurite = new AzuriteBuilder()
            .WithName($"azurite-{instance:D3}")
            .WithNetwork(_network).WithNetworkAliases($"azurite-{instance:D3}")
            .Build();

        var waitFor = Wait.ForUnixContainer()
            .UntilMessageIsLogged(RegexPatterns.EventHubReadyPattern());

        _eventhub = new ContainerBuilder()
            .WithName($"eventhubs-emulator-{instance:D3}")
            .WithImage("mcr.microsoft.com/azure-messaging/eventhubs-emulator:latest")
            .WithPortBinding(5672)
            .WithResourceMapping(configBytes, "/Eventhubs_Emulator/ConfigFiles/Config.json")
            .WithEnvironment("BLOB_SERVER", $"azurite-{instance:D3}")
            .WithEnvironment("METADATA_SERVER", $"azurite-{instance:D3}")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithNetwork(_network)
            .WithNetworkAliases($"eventhubs-emulator-{instance:D3}")
            .WithWaitStrategy(waitFor)
            .Build();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _eventhub.StopAsync();
        await _azurite.StopAsync();
        await _network.DeleteAsync();
        await _network.DisposeAsync();
    }

    /// <summary>
    ///     Creates and starts an EventHubComposer with default settings.
    /// </summary>
    /// <remarks>
    ///     This method creates an EventHubComposer with a default configuration. The configuration includes a single EventHub
    ///     namespace,
    ///     a single EventHub entity, and a single consumer group. The EventHub entity has a partition count of 4.
    ///     The method also starts the EventHubComposer, which involves creating and starting an Azurite container and an
    ///     EventHubs emulator container.
    ///     The connection string for the EventHubs emulator is extracted from the logs of the emulator container.
    /// </remarks>
    /// <returns>
    ///     A Task that represents the asynchronous operation. The Task result contains the EventHubFixture that encapsulates
    ///     the EventHubComposer and the connection string.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the connection string is not found in the emulator container
    ///     logs.
    /// </exception>
    public static async Task<EventHubFixture> CreateAndStartWithDefaultSettingsAsync()
    {
        var config = new EventHubConfigurationBuilder()
            .AddEntity(e => e
                .WithName("eh1")
                .WithPartitionCount(4)
                .AddConsumerGroup("cg1"))
            .Build();

        var composer = new EventHubComposer(SerializeConfiguration(config.NamespaceConfiguration));
        return await composer.StartAsync();
    }

    public static async Task<EventHubFixture> CreateAndStartAsync(NamespaceConfiguration namespaceConfiguration)
    {
        var config = SerializeConfiguration(namespaceConfiguration);
        var composer = new EventHubComposer(config);
        return await composer.StartAsync();
    }

    private static byte[] SerializeConfiguration(NamespaceConfiguration configuration)
    {
        var config = new
        {
            UserConfig = new
            {
                NamespaceConfig = new[]
                {
                    new
                    {
                        Type = "EventHub",
                        Name = NamespaceConfiguration.NamespaceName,
                        Entities = configuration.EntityConfigurations.Select(e => new
                        {
                            Name = e.EntityName,
                            e.PartitionCount,
                            ConsumerGroups = e.ConsumerGroups.Select(g => new { g.Name }).ToArray()
                        })
                    }
                },
                LoggingConfig = new
                {
                    Type = "File"
                }
            }
        };

        return JsonSerializer.SerializeToUtf8Bytes(config);
    }

    private async Task<EventHubFixture> StartAsync()
    {
        await _network.CreateAsync();
        await _azurite.StartAsync();
        await _eventhub.StartAsync();

        var logs = await _eventhub.GetLogsAsync();
        var matches = RegexPatterns.ConnectionStringPattern().Match(logs.Stdout);
        if (!matches.Success) throw new InvalidOperationException("Connection string not found");

        return new EventHubFixture(this, matches.Groups["ConnectionString"].Value);
    }
}