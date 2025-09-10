using CircleUp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CircleUp.ViewComponents
{
    public class StoriesViewComponent:ViewComponent
    {
        private readonly AppDbContext _context;

        public StoriesViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var allStories = await _context.Stories
                .Where(n => n.DateCreated >= DateTime.UtcNow.AddHours(-2))
                .Include(s => s.User)
                .ToListAsync();
            return View(allStories);
        }
    }
}
