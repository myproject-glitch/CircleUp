using CircleUp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleUp.Data.Helpers
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext appDbContext)
        {
            if (!appDbContext.Users.Any() && !appDbContext.Posts.Any()) 
            {
                var newUser = new User()
                {
                    FullName = "Eugene Hemen",
                    ProfilePictureUrl = "https://imageio.forbes.com/specials-images/imageserve/626c7cf3616c1112ae834a2b/0x0.jpg?format=jpg&crop=1603,1603,x1533,y577,safe&height=416&width=416&fit=bounds"
                };
                await  appDbContext.Users.AddAsync(newUser);
                await  appDbContext.SaveChangesAsync();

                var newPostWithoutImage = new Post()
                {
                    Content = "This is going to be our first post which is being loaded from the database" +
                    "and it has been created using our test user",
                    ImageUrl = "",
                    NrOfReports = 0,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    UserId = newUser.Id
                };

                var newPostWithImage = new Post()
                {
                    Content = "This is going to be our first post which is being loaded from the database" +
                    "and it has been created using our test user. This post has an image",
                    ImageUrl = "https://media.gettyimages.com/id/2213939595/photo/al-nassr-v-al-ittihad-saudi-pro-league.jpg?s=612x612&w=gi&k=20&c=RGRAOkYK7gbskfB7bMdPhb_STksqkGUXY7qugWvbYWs=",
                    NrOfReports = 0,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    UserId = newUser.Id
                };
                await appDbContext.Posts.AddRangeAsync(newPostWithoutImage,newPostWithImage);
                await appDbContext.SaveChangesAsync();
            }
        }
    }
}
