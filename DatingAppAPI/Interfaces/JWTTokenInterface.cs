using DatingAppAPI.Entities;

namespace DatingAppAPI.Interfaces
{
    public interface IJWTTokenInterface
    {
        string CreateToken(AppUser user);
    }
}
