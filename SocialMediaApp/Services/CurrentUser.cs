using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SocialMediaApp.Services;

internal sealed class CurrentUser(AuthenticationStateProvider authenticationStateProvider) : ICurrentUser
{
    public async Task<CurrentUserInfo?> GetAsync()
    {
        var principal = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return principal.Identity?.IsAuthenticated == true && id is not null
            ? new CurrentUserInfo(id, principal.FindFirstValue(ClaimTypes.Email) ?? principal.Identity.Name)
            : null;
    }
}
