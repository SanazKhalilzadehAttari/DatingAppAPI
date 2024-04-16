
using AutoMapper;
using DatingAppAPI.Data;
using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Extnsions;
using DatingAppAPI.Helpers;
using DatingAppAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace DatingAppAPI.Controllers
{
    [Authorize]
    public class UserController : APIBaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UserController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
        {
            _uow = uow;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var gender = await _uow.UserRepository.GetUserGender(User.getUser());
            userParams.CurrentUserName = User.getUser();
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";

            }
            var users = await _uow.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,
                users.PageSize, users.TotalCount, users.TotalPages));
            return Ok(users);
        }

        [HttpGet("{username}")]
        //[AllowAnonymous]
        public async Task<ActionResult<AppUser>> GetUser(string username)
        {
            return Ok(await _uow.UserRepository.GetMemberAsync(username));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.getUser());
            if (user == null) return NotFound();
            _mapper.Map(memberUpdateDTO, user);
            _uow.UserRepository.Update(user);
            return Ok();


        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.getUser());
            if (user == null) return NotFound();
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo()
            {
                Url = result.Url,
                PublicId = result.PublicId
            };
            if (user.Photos.Count == 0) photo.IsMain = true;
            user.Photos.Add(photo);
            if (await _uow.Complete())
            {
                return CreatedAtAction(nameof(GetUser),
                    new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("problem adding photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.getUser());

            if (user == null) return NotFound();
            var photo = user.Photos.SingleOrDefault(x=> x.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("this is already your main photo");
            var currentMainPhoto = user.Photos.SingleOrDefault(x => x.IsMain);
            if (currentMainPhoto != null)
            {
                currentMainPhoto.IsMain = false;
            }
            photo.IsMain = true;

            if(await _uow.Complete()) { return NoContent(); }

            return BadRequest("Problem setting the main photo");


        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.getUser()); 
            if (user == null) return NotFound();

            var photo =user.Photos.First(x => x.Id == photoId);
            if(photo == null) return NotFound();
            if(photo.IsMain) return BadRequest("you can not delete your main Photo");
            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest($"Error: {result.Error.Message}");
            }
            user.Photos.Remove(photo);
            if(await _uow.Complete()) { return Ok(); }
            return BadRequest("problem deleting photo");
        }
    }
}
