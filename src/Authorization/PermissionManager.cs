namespace Monolith;

public class PermissionManager
{
    /// <summary>
    /// default all (include undefined) permissions are valid
    /// if you want to restrict permissions, override this method
    ///     and register your own PermissionManager with .AddPermissionStores<T>
    /// </summary>
    public virtual Task<bool> IsValidAsync(string permission) => Task.FromResult(true);
}
