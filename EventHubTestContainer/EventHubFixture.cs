namespace EventHubTestContainer;

public sealed class EventHubFixture : IAsyncDisposable
{
    private readonly EventHubComposer _composer;

    internal EventHubFixture(EventHubComposer composer, string connectionString)
    {
        _composer = composer;
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; set; }

    public async ValueTask DisposeAsync()
    {
        await ((IAsyncDisposable)_composer).DisposeAsync();
    }
}