using DatingAppAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingAppAPI.Controllers
{
    public class AdminController : APIBaseController
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
       [Authorize(Policy="RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users.OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(u => u.Role.Name).ToList(),
                }).ToListAsync();

            return Ok(users);
        }
        [Authorize(Policy="RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult>EditRoles(string username,[FromQuery] string roles)
        {
            if(string.IsNullOrEmpty(roles)) { return BadRequest("You must select at least one role"); }
            var selectRoles = roles.Split(',').ToArray();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) { return NotFound(); }
            var userRoles = await _userManager.GetRolesAsync(user);
            var a = selectRoles.Except(userRoles);
            var result = await _userManager.AddToRolesAsync(user, selectRoles.Except(userRoles));
            if(!result.Succeeded) { return BadRequest("Filed to add to roles"); }
            result = await _userManager.RemoveFromRolesAsync(user,userRoles.Except(selectRoles));
            if(!result.Succeeded) { return BadRequest("Failed to Remove from Roles"); }

            return Ok(await _userManager.GetRolesAsync(user));
        }


        [Authorize(Policy="ModarateAdminPhotos")]
        [HttpGet("photos-to-modarator")]
        public ActionResult GetPhotoForModaration() {
            return Ok("only admin and moderator can see this");
        
        }
    }
}
