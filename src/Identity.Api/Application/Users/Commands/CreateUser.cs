using Light.Mediator;
using StarterKit.Identity.Api.Events;
using StarterKit.Identity.Contracts;
using StarterKit.Identity.Contracts.Services;

namespace StarterKit.Identity.Api.Application.Users.Commands;

public record CreateUserCommand : CreateUserRequest, ICommand<IResult<string>>;

internal class CreateUserCommandHandler(
    IUserService userService,
    IPublisher publisher)
    : ICommandHandler<CreateUserCommand, IResult<string>>
{
    public async Task<IResult<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await userService.CreateAsync(request).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            await publisher.Publish(
                new UserCreatedEvent(result.Data, request.UserName, request.Email),
                cancellationToken);
        }

        return result;
    }
}
