using System.Security.Claims;

namespace DatingAppAPI.Extnsions
{
    public static class ClaimPrincipleExtentions
    {
        public static string getUser(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }
        public static int getUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
