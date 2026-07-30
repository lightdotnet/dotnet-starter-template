using Light.Domain;
using Light.Domain.Entities.Interfaces;
using Microsoft.AspNetCore.Identity;
using StarterKit.Shared;

namespace StarterKit.Identity.Api.Entities;

public class User : IdentityUser, IEntity<string>, IAuditable, ISoftDelete
{
    public User() => Id = LightId.NewId();

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public ActiveStatus Status { get; set; } = new();

    public string? AuthProvider { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTimeOffset? Deleted { get; set; }

    public string? DeletedBy { get; set; }

    public void UpdateInfo(string? firstName, string? lastName, string? phoneNumber, string? email)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;
    }

    public void UpdateStatus(ActiveStatus.State status)
    {
        // only update 2 status
        if (status == ActiveStatus.State.Active || status == ActiveStatus.State.Locked)
            Status.Update(status);
    }

    public void ChangeAuthProvider(string? authProvider)
    {
        // auth user via other provider instead local password
        AuthProvider = string.IsNullOrEmpty(authProvider)
            ? null
            : authProvider;
    }

    public void Delete()
    {
        UserName = null;
        FirstName = null;
        LastName = null;
        PhoneNumber = null;
        Email = null;
        PasswordHash = null;
        AuthProvider = null;
        Status.Update(ActiveStatus.State.Locked);
    }
}
