namespace StarterKit.Endpoints;

public abstract class ApiControllerBase : Light.AspNetCore.Mvc.ApiControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}
