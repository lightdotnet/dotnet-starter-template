using StarterKit;
using System.Text.Json;
using Xunit;

namespace Framework.Tests.Shared;

public class StatusTests
{
    [Fact]
    public void DefaultConstructor_ShouldDefaultToActive()
    {
        // Act
        var status = new Status();

        // Assert
        Assert.Equal(Status.ActiveStatus.Active, status.Value);
        Assert.True(status.IsActive);
    }

    [Theory]
    [InlineData(Status.ActiveStatus.Inactive, false, true, false)]
    [InlineData(Status.ActiveStatus.Active, true, false, false)]
    [InlineData(Status.ActiveStatus.Locked, false, false, true)]
    public void Flags_ShouldReflectValue(Status.ActiveStatus value, bool isActive, bool isInactive, bool isLocked)
    {
        // Act
        var status = new Status(value);

        // Assert
        Assert.Equal(isActive, status.IsActive);
        Assert.Equal(isInactive, status.IsInactive);
        Assert.Equal(isLocked, status.IsLocked);
    }

    [Fact]
    public void Update_ShouldMutateValueAndFlags()
    {
        // Arrange
        var status = new Status(Status.ActiveStatus.Active);

        // Act
        status.Update(Status.ActiveStatus.Locked);

        // Assert
        Assert.Equal(Status.ActiveStatus.Locked, status.Value);
        Assert.True(status.IsLocked);
        Assert.False(status.IsActive);
    }

    [Fact]
    public void Equals_ShouldBeTrue_WhenValueIsTheSame()
    {
        // Arrange
        var a = new Status(Status.ActiveStatus.Locked);
        var b = new Status(Status.ActiveStatus.Locked);

        // Assert
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_ShouldBeFalse_WhenValueDiffers()
    {
        // Arrange
        var a = new Status(Status.ActiveStatus.Active);
        var b = new Status(Status.ActiveStatus.Locked);

        // Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Serialize_ShouldOnlyIncludeValue()
    {
        // Arrange
        var status = new Status(Status.ActiveStatus.Active);

        // Act
        var json = JsonSerializer.Serialize(status);

        // Assert
        Assert.Contains("\"Value\"", json);
        Assert.DoesNotContain("IsActive", json);
        Assert.DoesNotContain("IsInactive", json);
        Assert.DoesNotContain("IsLocked", json);
    }
}
