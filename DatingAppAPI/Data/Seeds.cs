using DatingAppAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DatingAppAPI.Data
{
    public class Seeds
    {
        public static async Task SeedUsers(DataContext context)
        {
            if (await context.Users.AnyAsync()) return;
            var userData = await File.ReadAllTextAsync("Data/UserDataSeed.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();
                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("pass"));
                user.PasswordSalt = hmac.Key;

                context.Users.Add(user);

            }
            await context.SaveChangesAsync();
        }
    }

}
