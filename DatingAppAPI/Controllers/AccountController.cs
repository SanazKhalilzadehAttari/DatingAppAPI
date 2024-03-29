﻿using AutoMapper;
using DatingAppAPI.Data;
using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingAppAPI.Controllers
{
    public class AccountController : APIBaseController
    {
        private readonly DataContext _dataContext;
        private readonly IJWTTokenInterface _jWTToken;
        private readonly IMapper _mapper;

        public AccountController(DataContext dataContext,
            IJWTTokenInterface jWTToken,IMapper mapper)
        {
            _dataContext = dataContext;
            _jWTToken = jWTToken;
            _mapper = mapper;
        }
        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await IsUserExist(registerDTO.Username.ToLower())) return BadRequest("the user is existed");
            var user = _mapper.Map<AppUser>(registerDTO);
            using var hmac = new HMACSHA512();
            user.UserName= registerDTO.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key;
            
            _dataContext.Add(user);
            await _dataContext.SaveChangesAsync();

            return new UserDTO
            {
                Username = user.UserName,
                Token = _jWTToken.CreateToken(user),
                KnownAs= user.KnownAs,
                Gender= user.Gender
              
            };
        }
        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _dataContext.Users.Include(p => p.Photos).FirstOrDefaultAsync(x=> x.UserName == loginDTO.Username);
            if (user == null) return Unauthorized("invalid user");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            for(int i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHash[i]) return Unauthorized("invalid Password"); 
            }
            return new UserDTO
            {
                Username = user.UserName,
                Token = _jWTToken.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(y => y.IsMain)?.Url,
                KnownAs= user.KnownAs,
                Gender = user.Gender,
            };
        }
       private async Task<bool> IsUserExist(string username)
        {
            return await _dataContext.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
