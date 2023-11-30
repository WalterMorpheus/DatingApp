using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository : ILikesRepository
{
    private readonly DataContext _context;

    public LikesRepository(DataContext context)
    {
        _context = context;
    }
    public async Task<UserLike> GetUserLike(int sourceUserId, int targerUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId,targerUserId);
    }
    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
        var user = _context.Users.OrderBy(u=> u.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();

        if(likesParams.Predicate == "liked")
        {
            likes = likes.Where(likes => likes.SourceUserId == likesParams.UserId);
            user = likes.Select(likes => likes.TargetUser);
        }

        if(likesParams.Predicate == "likedBy")
        {
            likes = likes.Where(likes => likes.TargerUserId == likesParams.UserId);
            user = likes.Select(likes => likes.SourceUser);
        }

        var  likedUsers = user.Select(user => new LikeDto{
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalcuateAge(),
            PhotoUrl = user.Photos.FirstOrDefault(x=> x.IsMain).Url,
            City = user.City,
            Id = user.Id
        });

        return await PagedList<LikeDto>.CreateAsync(likedUsers,likesParams.PageNumber,likesParams.PageSize);
    }
    public async Task<AppUser> GetUserWithLikes(int userId)
    {
        return await _context.Users
            .Include(x=> x.LikedUsers)
            .FirstOrDefaultAsync(x=>x.Id == userId);
    }
}
