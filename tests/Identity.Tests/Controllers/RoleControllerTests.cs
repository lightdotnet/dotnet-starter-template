using Light.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StarterKit.Identity.Api.Controllers;
using StarterKit.Identity.Contracts;
using StarterKit.Identity.Contracts.Services;
using Xunit;

namespace Identity.Tests.Controllers;

public class RoleControllerTests
{
    private static (RoleController Controller, Mock<IRoleService> RoleService) CreateSut()
    {
        var roleServiceMock = new Mock<IRoleService>();
        var controller = new RoleController(roleServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        return (controller, roleServiceMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAllRoles()
    {
        // Arrange: plain IEnumerable<RoleDto> is not a ResultBase, so ApiControllerBase.Ok wraps it
        // in a Result<IEnumerable<RoleDto>> before returning it.
        var (controller, roleServiceMock) = CreateSut();
        var expected = new List<RoleDto> { new() { Id = "1", Name = "Admin" } };
        roleServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(expected);

        // Act
        var response = await controller.GetAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        var wrapped = Assert.IsType<Result<IEnumerable<RoleDto>>>(objectResult.Value);
        Assert.Same(expected, wrapped.Data);
    }

    [Fact]
    public async Task GetAsync_ById_ShouldReturnServiceResult()
    {
        // Arrange
        var (controller, roleServiceMock) = CreateSut();
        var expected = Result<RoleDto>.NotFound("Role missing not found");
        roleServiceMock.Setup(s => s.GetByIdAsync("missing")).ReturnsAsync(expected);

        // Act
        var response = await controller.GetAsync("missing");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }

    [Fact]
    public async Task CreateAsync_ShouldDelegateToService()
    {
        // Arrange
        var (controller, roleServiceMock) = CreateSut();
        var request = new CreateRoleRequest { Name = "Admin" };
        var expected = Result<string>.Success("role-1");
        roleServiceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(expected);

        // Act
        var response = await controller.CreateAsync(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }

    [Fact]
    public async Task UpdateAsync_ShouldDelegateToService()
    {
        // Arrange
        var (controller, roleServiceMock) = CreateSut();
        var request = new RoleDto { Id = "role-1", Name = "Admin" };
        var expected = Result.Success();
        roleServiceMock.Setup(s => s.UpdateAsync(request)).ReturnsAsync(expected);

        // Act
        var response = await controller.UpdateAsync(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDelegateToService()
    {
        // Arrange
        var (controller, roleServiceMock) = CreateSut();
        var expected = Result.Error("Role has already setup claims.");
        roleServiceMock.Setup(s => s.DeleteAsync("role-1")).ReturnsAsync(expected);

        // Act
        var response = await controller.DeleteAsync("role-1");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }
}
