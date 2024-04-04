using AutoMapper;
using DatingAppAPI.Data;
using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingAppAPI.Controllers
{
    public class AccountController : APIBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJWTTokenInterface _jWTToken;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager,
            IJWTTokenInterface jWTToken,IMapper mapper)
        {
            _userManager = userManager;
            _jWTToken = jWTToken;
            _mapper = mapper;
        }
        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await IsUserExist(registerDTO.Username.ToLower())) return BadRequest("the user is existed");
            var user = _mapper.Map<AppUser>(registerDTO);
           /* using var hmac = new HMACSHA512();
            user.UserName= registerDTO.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key;*/
            
           var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if(!result.Succeeded) { return BadRequest(result.Errors); }
            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if(!roleResult.Succeeded) return BadRequest(roleResult.Errors);
            return new UserDTO
            {
                Username = user.UserName,
                Token = await _jWTToken.CreateToken(user),
                KnownAs= user.KnownAs,
                Gender= user.Gender
              
            };
        }
        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(x=> x.UserName == loginDTO.Username);
            if (user == null) return Unauthorized("invalid user");
           /* using var hmac = new HMACSHA512(user.PasswordSalt);
            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            for(int i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHash[i]) return Unauthorized("invalid Password"); 
            }*/
           var result = await _userManager.CheckPasswordAsync(user,loginDTO.Password);
            if (!result) return Unauthorized("invalid password"); 
            return new UserDTO
            {
                Username = user.UserName,
                Token =await _jWTToken.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(y => y.IsMain)?.Url,
                KnownAs= user.KnownAs,
                Gender = user.Gender,
            };
        }
       private async Task<bool> IsUserExist(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
