using CircleUp.Data;
using CircleUp.Data.Models;
using CircleUp.Data.Services;
using CircleUp.ViewModels.Stories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CircleUp.Data.Helpers.Enums;

namespace CircleUp.Controllers
{
    public class StoriesController : Controller
    {
        private readonly IStoriesServices _storiesServices;
        private readonly IFilesService _filesService;

        public StoriesController(IStoriesServices storiesServices, IFilesService filesService)
        {
            _storiesServices = storiesServices;
            _filesService = filesService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateStory(StoryVM storyVM)
        {
            //Get logges in user
            int loggedInUser = 1;

            var imageUploadPath = await _filesService
                .UploadImageAsync(storyVM.Image,ImageFileType.StoryImage);


            //Create a new post

            var newStory = new Story
            {
               
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                ImageUrl = imageUploadPath,
                UserId = loggedInUser
            };

            await _storiesServices.CreateStoryAsync(newStory);
            return RedirectToAction("Index","Home");

        }
    }
}
