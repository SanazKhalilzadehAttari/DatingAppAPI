using DatingAppAPI.Entities;

namespace DatingAppAPI.Interfaces
{
    public interface IJWTTokenInterface
    {
        Task<string> CreateToken(AppUser user);
    }
}
