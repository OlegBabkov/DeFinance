using System.Security.Claims;
using DeFinance.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace DeFinance.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}
