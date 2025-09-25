using System.Diagnostics;
using CircleUp.Data;
using CircleUp.Data.Helpers;
using CircleUp.Data.Helpers.Enums;
using CircleUp.Data.Models;
using CircleUp.Data.Services;
using CircleUp.ViewModels.Home;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CircleUp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IPostsService _postsService;
        private readonly IHashtagsService _hashtagsService;
        private readonly IFilesService _filesService;

        public HomeController(ILogger<HomeController> logger,IPostsService postsService,
            IHashtagsService hashtagsService, IFilesService filesService)
        {
            _logger = logger;
            _postsService = postsService;
            _hashtagsService = hashtagsService;
            _filesService = filesService;
        }
        public async Task<IActionResult> IndexAsync()
        {
            int loggedInUserId = 1;

            var allPosts = await _postsService.GetAllPostsAsync(loggedInUserId);
            return View(allPosts);
        }


        public async Task<IActionResult> Details(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM? post)
        {
            if (post == null || string.IsNullOrWhiteSpace(post.Content))
                return BadRequest("Post content is required.");


            //Get logges in user
            int loggedInUser = 1;

            var imageUploadPath = await _filesService
                .UploadImageAsync(post.Image, ImageFileType.PostImage);

            //Create a new post

            var newPost = new Post
            {
                Content = post.Content,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                ImageUrl = imageUploadPath,
                NrOfReports = 0,
                UserId = loggedInUser
            };
            await _postsService.CreatePostsAsync(newPost);
            // Process hashtags only if content is not null/empty
            if (!string.IsNullOrWhiteSpace(post.Content))
            {
                await _hashtagsService.ProcessHashtagsForNewPostAsync(post.Content);
            }

            //Find and store hashtags to database

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostLikes(PostLikeVM postLikeVM)
        {
            int loggedInUserId = 1;

            //check if the user already like the post

            await _postsService.TogglePostLikeAsync(postLikeVM.PostId, loggedInUserId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPostComment(PostCommentVM postCommentVM)
        {
            int loggedInUserId = 1;

            //Create a post object
            var newComment = new Comment()
            {
                UserId = loggedInUserId,
                PostId = postCommentVM.PostId,
                Content = postCommentVM.Content,
                DateCreated = DateTime.UtcNow,
                DatetUpdated = DateTime.UtcNow,
            };
            await _postsService.AddPostCommentAsync(newComment);
            return RedirectToAction("Index");

        }


        [HttpPost]
        public async Task<IActionResult> RemovePostComment(RemoveCommentVM removeCommentVM)
        {

            await _postsService.RemovePostCommentAsync(removeCommentVM.CommentId);
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> TogglePostFavorite(PostFavoriteVM postFavoriteVM)
        {
            int loggedInUserId = 1;

            //check if the user already favorite the post

            await _postsService.TogglePostFavoriteAsync(postFavoriteVM.PostId, loggedInUserId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostVisibilty(PostVisibilityVM postVisibiltyVM)
        {
            int loggedInUserId = 1;

            //get post by id & loggedin user id
            await _postsService.TogglePostVisibilityAsync(postVisibiltyVM.PostId, loggedInUserId);
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> AddPostReport(PostReportVM postReportVM)
        {
            int loggedInUserId = 1;

            await _postsService.ReportPostAsync(postReportVM.PostId, loggedInUserId);
            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> PostRemove(PostRemoveVM postRemoveVM)
        {
            if (postRemoveVM == null)
                return BadRequest("Invalid request.");
            var postRemoved = await _postsService.RemovePostAsync(postRemoveVM.PostId);
            if (postRemoved == null)
                return NotFound("Post not found.");

            if (!string.IsNullOrWhiteSpace(postRemoved.Content))
            {
                await _hashtagsService.ProcessHashtagsForRemovePostAsync(postRemoved.Content);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Error(int? code = null)
        {
            string requestId = HttpContext.TraceIdentifier;

            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown Agent";
            string requestPath = HttpContext.Request.Path.Value ?? "Unknown Path";

            // Pass data to the view
            ViewBag.RequestId = requestId;
            ViewBag.RequestPath = requestPath;
            ViewBag.ClientIp = clientIp;
            ViewBag.UserAgent = userAgent;
            ViewBag.StatusCode = code ?? 500;

            return View();
        }


    }
}