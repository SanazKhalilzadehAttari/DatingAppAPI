using DatingAppAPI.Data;
using DatingAppAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingAppAPI.Controllers
{
  
    public class UserController : APIBaseController
    {
        private readonly DataContext _context;
        public UserController(DataContext dataContext)
        {
            _context = dataContext;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}
