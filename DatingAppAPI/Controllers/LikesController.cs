using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Extnsions;
using DatingAppAPI.Helpers;
using DatingAppAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DatingAppAPI.Controllers
{
    public class LikesController : APIBaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository,
            ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.getUserId();
            var likedUser = await _userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);
            if (likedUser == null) { return NotFound(); }
            if (sourceUser.UserName == username) return BadRequest("you can not like yourself");
            var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null) { return BadRequest("you already liked this user"); }
            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id,
            };
            sourceUser.LikedUsers.Add(userLike);
           
            if (await _userRepository.SaveAsync()) return Ok();
            return BadRequest("Failed to like users");

        }


        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDTO>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.getUserId();
            var users = await _likesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}

