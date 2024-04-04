using DatingAppAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DatingAppAPI.Data
{
    public class Seeds
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;
            var userData = await File.ReadAllTextAsync("Data/UserDataSeed.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

            var roles = new List<AppRole>
            {
                new AppRole{Name ="Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name ="Modarator"}
            };
            foreach(var role in roles)
            {
                IdentityResult result = await roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                        Console.WriteLine($"Oops! {error.Description} ({error.Code})");
                }
            }
            foreach (var user in users)
            {
                /*using var hmac = new HMACSHA512();
                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("pass"));
                user.PasswordSalt = hmac.Key;*/
                IdentityResult result = await userManager.CreateAsync(user, "Password0");
                if (!result.Succeeded) {
                    foreach (IdentityError error in result.Errors)
                        Console.WriteLine($"Oops! {error.Description} ({error.Code})");
                  }
                await userManager.AddToRoleAsync(user, "Member");
            }
            // await userManager.SaveChangesAsync();

            var admin = new AppUser
            {
                UserName = "admin"
            };
            await userManager.CreateAsync(admin, "Password0");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Modarator" });
        }
    }

}
