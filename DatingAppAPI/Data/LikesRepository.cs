using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Extnsions;
using DatingAppAPI.Helpers;
using DatingAppAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingAppAPI.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context) {
        _context = context;
        
        }
        public async Task<UserLike> GetUserLike(int sourceUseId, int targetUseId)
        {
           return await _context.Likes.FindAsync(sourceUseId, targetUseId);
        }

        public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
        {
            var users =  _context.Users.OrderBy(u=> u.UserName).AsQueryable();
            var liked = _context.Likes.AsQueryable();
            if(likesParams.Predicate == "Liked")
            {
                 liked = liked.Where(l => l.SourceUserId == likesParams.UserId);
                 users = liked.Select(l => l.TargetUser);

            }
            if (likesParams.Predicate == "LikedBy")
            {
                 liked = liked.Where(l => l.TargetUserId == likesParams.UserId);
                 users = liked.Select(l => l.SourceUser);

            }
            var likedUser = users.Select(user => new LikeDTO
            {
                Id = user.Id,
                Age =  user.DateOfBirth.CalculateAge(),
                photoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                city = user.City,
                KnownAs = user.KnownAs,
                Username = user.UserName

            });

            return await PagedList<LikeDTO>.CreateAsync(likedUser, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users.Include(u => u.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
