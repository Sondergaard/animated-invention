using EventHubTestContainer.Configuration;

namespace Tests;

public class EventHubConfigurationBuilderTests
{
    [Fact]
    public void Build_ShouldReturnCorrectConfiguration()
    {
        // Arrange
        var builder = new EventHubConfigurationBuilder();
        builder.AddEntity(e => e.WithName("entity1").AddConsumerGroup("cg1"));

        // Act
        var config = builder.Build();

        // Assert
        Assert.Equal("entity1", config.NamespaceConfiguration.EntityConfigurations[0].EntityName);
    }
    
    [Fact]
    public void AddMoreThen10Entities_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new EventHubConfigurationBuilder();

        // Act and Assert
        var exception = Assert.Throws<InvalidOperationException>(AddMoreThen10Entities);
        Assert.Equal($"A maximum of {NamespaceConfiguration.MaxNoOfEntities} entities can be added.", exception.Message);
        return;

        void AddMoreThen10Entities()
        {
            for (var i = 0; i < NamespaceConfiguration.MaxNoOfEntities + 1; i++)
            {
                var entityNo = i;
                builder.AddEntity(e => e.WithName($"entity{entityNo}").AddConsumerGroup($"cg{entityNo}"));
            }
            builder.Build();
        }
    }
    
    [Fact]
    public void AddConsumerGroup_WithReservedName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new EventHubConfigurationBuilder();

        // Act and Assert
        var exception = Assert.Throws<InvalidOperationException>(AddConsumerGroupWithReservedName);
        Assert.Equal($"Consumer group name '{ConsumerGroup.ReservedConsumerGroupName}' is reserved.", exception.Message);
        return;

        void AddConsumerGroupWithReservedName()
        {
            builder.AddEntity(e => e.WithName("entity1").AddConsumerGroup(ConsumerGroup.ReservedConsumerGroupName));
            builder.Build();
        }
    }

    [Fact]
    public void AddConsumerGroups_ExceedingMaximum_ShouldThrowInvalidOperationException()
    {
        EventHubConfigurationBuilder builder = new();
        
        var exception = Assert.Throws<InvalidOperationException>(AddConsumerGroupsExceedingMaximum);
        Assert.Equal($"A maximum of {EntityConfigurationBuilder.MaxConsumerGroups} consumer groups can be added.", exception.Message);
        return;
        
        void AddConsumerGroupsExceedingMaximum()
        {
            builder.AddEntity(e =>
            {
                e.WithName("entity1");
                for (var i = 0; i < EntityConfigurationBuilder.MaxConsumerGroups + 1; i++)
                {
                    var consumerGroupNo = i;
                        e.AddConsumerGroup($"cg{consumerGroupNo}");
                }
            });
            builder.Build();
        }
    }
    
    [Fact]
    public void CreatingEntity_WhenNoConsumerGroupsAdded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new EventHubConfigurationBuilder();

        // Act and Assert
        var exception = Assert.Throws<InvalidOperationException>(() => builder.AddEntity(e => e.WithName("entity1")).Build());
        Assert.Equal("At least one consumer group must be added.", exception.Message);
    }
}