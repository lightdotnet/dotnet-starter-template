using Light.ActiveDirectory.Interfaces;
using Light.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StarterKit.Identity.Api.Controllers;
using StarterKit.Identity.Contracts;
using StarterKit.Identity.Contracts.Services;
using StarterKit.Shared;
using Xunit;

namespace Identity.Tests.Controllers;

public class UserControllerTests
{
    private static (UserController Controller, Mock<IUserService> UserService, Mock<IActiveDirectoryService> ActiveDirectory) CreateSut()
    {
        var userServiceMock = new Mock<IUserService>();
        var adServiceMock = new Mock<IActiveDirectoryService>();
        var controller = new UserController(userServiceMock.Object, adServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        return (controller, userServiceMock, adServiceMock);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnServiceResult()
    {
        // Arrange: PagedResult<T> derives from ResultBase, so Ok(...) passes it through unwrapped.
        var (controller, userServiceMock, _) = CreateSut();
        var search = new SearchUserQuery { SearchValue = "jane" };
        var page = new PageQuery { PageNumber = 2, PageSize = 5 };
        var expected = new PagedResult<UserDto>([], 2, 5, 0);
        userServiceMock.Setup(s => s.SearchAsync(search, 2, 5)).ReturnsAsync(expected);

        // Act
        var response = await controller.SearchAsync(search, page);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAllUsers()
    {
        // Arrange: plain IEnumerable<UserDto> is not a ResultBase, so ApiControllerBase.Ok wraps it
        // in a Result<IEnumerable<UserDto>> before returning it.
        var (controller, userServiceMock, _) = CreateSut();
        var expected = new List<UserDto> { new() { Id = "1", UserName = "jane" } };
        userServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(expected);

        // Act
        var response = await controller.GetAsync();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        var wrapped = Assert.IsType<Result<IEnumerable<UserDto>>>(objectResult.Value);
        Assert.Same(expected, wrapped.Data);
    }

    [Fact]
    public async Task GetAsync_ById_ShouldReturnServiceResult()
    {
        // Arrange
        var (controller, userServiceMock, _) = CreateSut();
        var expected = Result<UserDto>.NotFound("User missing not found");
        userServiceMock.Setup(s => s.GetByIdAsync("missing")).ReturnsAsync(expected);

        // Act
        var response = await controller.GetAsync("missing");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnServiceResult()
    {
        // Arrange
        var (controller, userServiceMock, _) = CreateSut();
        var expected = Result<UserDto>.Success(new UserDto { Id = "1", UserName = "jane" });
        userServiceMock.Setup(s => s.GetByUserNameAsync("jane")).ReturnsAsync(expected);

        // Act
        var response = await controller.GetByUsernameAsync("jane");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }

    [Fact]
    public async Task PutAsync_ShouldReturnError_WhenRouteAndBodyIdsDoNotMatch()
    {
        // Arrange
        var (controller, userServiceMock, _) = CreateSut();
        var request = new UserDto { Id = "other-id", UserName = "jane" };

        // Act
        var response = await controller.PutAsync("route-id", request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        var result = Assert.IsType<Result>(objectResult.Value);
        Assert.False(result.IsSuccess);
        userServiceMock.Verify(s => s.UpdateAsync(It.IsAny<UserDto>()), Times.Never);
    }

    [Fact]
    public async Task PutAsync_ShouldDelegateToService_WhenIdsMatch()
    {
        // Arrange
        var (controller, userServiceMock, _) = CreateSut();
        var request = new UserDto { Id = "user-1", UserName = "jane" };
        var expected = Result.Success();
        userServiceMock.Setup(s => s.UpdateAsync(request)).ReturnsAsync(expected);

        // Act
        var response = await controller.PutAsync("user-1", request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDelegateToService()
    {
        // Arrange
        var (controller, userServiceMock, _) = CreateSut();
        var expected = Result.Success();
        userServiceMock.Setup(s => s.DeleteAsync("user-1")).ReturnsAsync(expected);

        // Act
        var response = await controller.DeleteAsync("user-1");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }

    [Fact]
    public async Task ForcePasswordAsync_ShouldDelegateToService()
    {
        // Arrange
        var (controller, userServiceMock, _) = CreateSut();
        var expected = Result.Success();
        userServiceMock.Setup(s => s.ForcePasswordAsync("user-1", "new-pass")).ReturnsAsync(expected);

        // Act
        var response = await controller.ForcePasswordAsync("user-1", "new-pass");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Same(expected, objectResult.Value);
    }
}
