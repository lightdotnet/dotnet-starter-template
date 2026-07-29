namespace StarterKit.Identity.Services;

public interface IServiceClaimService
{
    /// <summary>
    /// Get all registered service claims
    /// </summary>
    Task<IEnumerable<ServiceClaimDto>> GetAllAsync();

    /// <summary>
    /// Get claims registered by a specific owner service
    /// </summary>
    Task<IEnumerable<ServiceClaimDto>> GetByOwnerServiceAsync(string ownerService);

    /// <summary>
    /// Register/replace the full set of claims for an owner service
    /// </summary>
    Task<IResult> RegisterAsync(RegisterServiceClaimsRequest request);
}
