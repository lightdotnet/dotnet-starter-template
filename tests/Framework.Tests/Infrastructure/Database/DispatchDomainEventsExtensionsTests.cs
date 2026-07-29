using Framework.Tests.Infrastructure.TestSupport;
using Light.Mediator;
using StarterKit.Database;
using Xunit;

namespace Framework.Tests.Infrastructure.Database;

public class DispatchDomainEventsExtensionsTests
{
    private sealed class RecordingPublisher : IPublisher
    {
        public List<INotification> Published { get; } = [];

        public Task Publish(INotification notification, CancellationToken cancellationToken = default)
        {
            Published.Add(notification);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchDomainEvents_ShouldPublishAndClear_QueuedEvents()
    {
        // Arrange
        using var context = TestDbContext.CreateInMemory();
        var aggregate = new TestAggregate();
        aggregate.RaiseTestEvent();
        context.Add(aggregate);
        var publisher = new RecordingPublisher();

        // Act
        await publisher.DispatchDomainEvents(context);

        // Assert
        var published = Assert.Single(publisher.Published);
        Assert.IsType<TestDomainEvent>(published);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public async Task DispatchDomainEvents_ShouldNotPublish_WhenNoEventsQueued()
    {
        // Arrange
        using var context = TestDbContext.CreateInMemory();
        var aggregate = new TestAggregate();
        context.Add(aggregate);
        var publisher = new RecordingPublisher();

        // Act
        await publisher.DispatchDomainEvents(context);

        // Assert
        Assert.Empty(publisher.Published);
    }
}
