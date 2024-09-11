using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipleExtension
{
    public static string GetUserName(this ClaimsPrincipal user)
    {
        var userName = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return userName ?? throw new Exception("Can't get userName from token");
    }
}
