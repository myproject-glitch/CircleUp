using CircleUp.Data;
using CircleUp.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CircleUp.ViewComponents
{
    public class StoriesViewComponent:ViewComponent
    {
        private readonly IStoriesServices _storiesServices;
        public StoriesViewComponent(IStoriesServices storiesServices)
        {
            _storiesServices = storiesServices;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var allStories = await _storiesServices.GetAllStoriesAsync();
            return View(allStories);
        }
    }
}
