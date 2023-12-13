using DatingAppAPI.Data;
using DatingAppAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace DatingAppAPI.Controllers
{
    public class AccountController : APIBaseController
    {
        private readonly DataContext _dataContext;

        public AccountController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [HttpPost("Register")]
        public async Task<ActionResult<AppUser>> Register(string username, string password)
        {
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                PasswordSalt = hmac.Key
            };
            _dataContext.Add(user);
            await _dataContext.SaveChangesAsync();

            return user;
        }

       
    }
}
