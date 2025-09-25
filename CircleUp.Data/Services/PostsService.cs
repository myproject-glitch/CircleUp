using CircleUp.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleUp.Data.Services
{
    public class PostsService : IPostsService
    {
        private readonly AppDbContext _context;
        public PostsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetAllPostsAsync(int loggedInUserId)
        {
            var allPosts = await _context.Posts
               .Where(n => (!n.IsPrivate || n.UserId == loggedInUserId) && n.Reports.Count < 5 && !n.IsDeleted)
               .Include(n => n.User)
               .Include(n => n.Likes)
               .Include(n => n.Favorites)
               .Include(n => n.Comments).ThenInclude(n => n.User)
               .Include(n => n.Reports)
               .OrderByDescending(d => d.DateCreated)
               .ToListAsync();

            return allPosts;
        }
        [HttpPost]
        public async Task<Post> GetPostByIdAsync(int postId)
        {
            var postDb = await _context.Posts
              .Include(n => n.User)
              .Include(n => n.Likes)
              .Include(n => n.Favorites)
              .Include(n => n.Comments).ThenInclude(n => n.User)
              .FirstOrDefaultAsync(n => n.Id == postId);

            return postDb;
        }


        public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId)
        {
            var allFavoritedPosts = await _context.Posts
                .Where(p => p.Favorites.Any(f => f.UserId == loggedInUserId)
                            && !p.IsDeleted
                            && p.Reports.Count < 5)
                .Include(p => p.Reports)
                .Include(p => p.User)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();

            return allFavoritedPosts;
        }

        public async Task AddPostCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<Post> CreatePostsAsync(Post post)
        {


            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
            return post;
        }


        public async Task<Post> RemovePostAsync(int postId)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(n => n.Id == postId);
            if (postDb != null)
            {
                // _context.Posts.Remove(postDb);
                postDb.IsDeleted = true;
                _context.Posts.Update(postDb);
                await _context.SaveChangesAsync();
            }

            return postDb;
        }

        public async Task RemovePostCommentAsync(int commentId)
        {
            var commentDb = await _context.Comments.FirstOrDefaultAsync(n => n.Id == commentId);
            if (commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReportPostAsync(int postId, int userId)
        {
            var newReport = new Report
            {
                Id = postId,
                UserId = userId,
                DateCreated = DateTime.Now
            };
            await _context.Reports.AddAsync(newReport);
            await _context.SaveChangesAsync();
        }

        public async Task TogglePostFavoriteAsync(int postId, int userId)
        {


            //check if the user already favorite the post

            var favorite = await _context.Favorites
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newFavorite = new Favorite()
                {
                    PostId = postId,
                    UserId = userId,
                    DateCreated = DateTime.UtcNow
                };
                await _context.Favorites.AddAsync(newFavorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TogglePostLikeAsync(int postId, int userId)
        {


            //check if the user already like the post

            var like = await _context.Likes
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();
            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newLike = new Like()
                {
                    PostId = postId,
                    UserId = userId
                };
                await _context.Likes.AddAsync(newLike);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TogglePostVisibilityAsync(int postId, int userId)
        {

            var post = await _context.Posts
                .FirstOrDefaultAsync(l => l.Id == postId && l.UserId == userId);
            if (post != null)

            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}
