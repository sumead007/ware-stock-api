using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Infrastructure.Identity;

namespace WareStockApi.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Matches the short claim types JwtTokenService mints ("sub"/"role"), not the legacy
    // System.Security.Claims.ClaimTypes.* URIs — see DependencyInjection.cs's MapInboundClaims = false.
    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
    public List<string>? Roles => _httpContextAccessor.HttpContext?.User?.FindAll(JwtTokenService.RoleClaimType).Select(x => x.Value).ToList();

}
