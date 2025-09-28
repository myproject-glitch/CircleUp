
using CircleUp.Data.Helpers.Enums;
using CircleUp.Data.Models;
using CircleUp.Data.Services;
using CircleUp.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CircleUp.Controllers.Base;

namespace CircleUp.Controllers
{
    [Authorize] // 🔒 All actions require login unless specified otherwise
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostsService _postsService;
        private readonly IHashtagsService _hashtagsService;
        private readonly IFilesService _filesService;

        public HomeController(
            ILogger<HomeController> logger,
            IPostsService postsService,
            IHashtagsService hashtagsService,
            IFilesService filesService)
        {
            _logger = logger;
            _postsService = postsService;
            _hashtagsService = hashtagsService;
            _filesService = filesService;
        }

        // ✅ Helper to get logged in UserId


        public async Task<IActionResult> Index()
        {
            var loggedInUserId = GetUserId();

            if (loggedInUserId == null)
                return RedirectToLogin();

            var allPosts = await _postsService.GetAllPostsAsync(loggedInUserId.Value);
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
            
                var loggedInUserId = GetUserId();

                if (loggedInUserId == null)
                    return RedirectToLogin();

                var allPosts = await _postsService.GetAllPostsAsync(loggedInUserId.Value);

                if (post == null || string.IsNullOrWhiteSpace(post.Content))
                return BadRequest("Post content is required.");

            var imageUploadPath = await _filesService.UploadImageAsync(post.Image, ImageFileType.PostImage);

            var newPost = new Post
            {
                Content = post.Content,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                ImageUrl = imageUploadPath,
                NrOfReports = 0,
                UserId = loggedInUserId.Value
            };

            await _postsService.CreatePostsAsync(newPost);

            if (!string.IsNullOrWhiteSpace(post.Content))
            {
                await _hashtagsService.ProcessHashtagsForNewPostAsync(post.Content);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostLikes(PostLikeVM postLikeVM)
        {

            var loggedInUserId = GetUserId();

            if (loggedInUserId == null)
                return RedirectToLogin();

            await _postsService.TogglePostLikeAsync(postLikeVM.PostId,loggedInUserId.Value);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPostComment(PostCommentVM postCommentVM)
        {

            var loggedInUserId = GetUserId();

            if (loggedInUserId == null)
                return RedirectToLogin();

            var newComment = new Comment()
            {
                UserId = loggedInUserId.Value,
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

            var loggedInUserId = GetUserId();

            if (loggedInUserId == null)
                return RedirectToLogin();

            await _postsService.TogglePostFavoriteAsync(postFavoriteVM.PostId,loggedInUserId.Value);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostVisibilty(PostVisibilityVM postVisibiltyVM)
        {

            var loggedInUserId = GetUserId();

            if (loggedInUserId == null)
                return RedirectToLogin();

            await _postsService.TogglePostVisibilityAsync(postVisibiltyVM.PostId, loggedInUserId.Value);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPostReport(PostReportVM postReportVM)
        {

            var loggedInUserId = GetUserId();

            if (loggedInUserId == null)
                return RedirectToLogin();

            await _postsService.ReportPostAsync(postReportVM.PostId, loggedInUserId.Value);
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

            ViewBag.RequestId = requestId;
            ViewBag.RequestPath = HttpContext.Request.Path.Value ?? "Unknown Path";
            ViewBag.ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
            ViewBag.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown Agent";
            ViewBag.StatusCode = code ?? 500;

            return View();
        }

    }
}
