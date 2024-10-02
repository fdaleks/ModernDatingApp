using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipleExtension
{
    public static string GetUserName(this ClaimsPrincipal user)
    {
        var userName = user.FindFirstValue(ClaimTypes.Name) ?? throw new Exception("Can't get userName from token");
        return userName;
    }

    public static int GetUserId(this ClaimsPrincipal user)
    {
        int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Can't get id from token"));
        return userId;
    }
}
