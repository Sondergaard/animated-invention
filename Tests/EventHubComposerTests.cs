using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using EventHubTestContainer;

namespace Tests;

public class EventHubComposerTests
{
    private static async Task ProduceEventOnEventHub(EventHubProducerClient producerClient, byte[] messageContent)
    {
        var eventData = new EventData(messageContent);
        await producerClient.SendAsync(new []{ eventData });
    }

    [Fact]
    public async Task GivenEventHubComposer_WhenEventPublished_ThenItCanBeConsumed()
    {
        byte[] messageContent = Guid.NewGuid().ToByteArray();
        await using var fixture = await EventHubComposer.CreateAndStartWithDefaultSettingsAsync();
        await using var producerClient = new EventHubProducerClient(fixture.ConnectionString, "eh1");
        var consumerClient = new EventHubConsumerClient("cg1", fixture.ConnectionString, "eh1");

        await ProduceEventOnEventHub(producerClient, messageContent);
        
        try
        {
            await foreach (var eventData in consumerClient.ReadEventsAsync())
            {
                var data = eventData.Data.Body.ToArray();
                Assert.NotNull(data);
                Assert.Equal(messageContent, data);
                break; // stop after receiving one event
            }
        }
        catch (Exception)
        {
            Assert.Fail("Exception was thrown when trying to consume an event.");
        }
    }
}