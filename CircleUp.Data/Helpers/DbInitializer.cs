using CircleUp.Data.Helpers.Constants;
using CircleUp.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleUp.Data.Helpers
{
    public static class DbInitializer
    {
        public static async Task SeedUsersAndRolesAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            //Roles
            if(!roleManager.Roles.Any())
            {
                foreach(var roleName in AppRoles.All)
                {
                    if(!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                    }
                }
            }

            //Users with roles

            var userPassword = "Coding@1234?";
            var newUser = new User()
            {
                UserName = "Eugene.Hemen",
                Email = "eugene@hemen.com",
                FullName = "Eugene Hemen",
                ProfilePictureUrl = "https://imageio.forbes.com/specials-images/imageserve/626c7cf3616c1112ae834a2b/0x0.jpg?format=jpg&crop=1603,1603,x1533,y577,safe&height=416&width=416&fit=bounds",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newUser, userPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, AppRoles.User);


            };

            var newAdmin = new User()
            {
                UserName = "admin.admin",
                Email = "admin@eugene.com",
                FullName = "Eugene Hemen",
                ProfilePictureUrl = "https://imageio.forbes.com/specials-images/imageserve/626c7cf3616c1112ae834a2b/0x0.jpg?format=jpg&crop=1603,1603,x1533,y577,safe&height=416&width=416&fit=bounds",
                EmailConfirmed = true
            };

            var resultNewAdmin = await userManager.CreateAsync(newAdmin, userPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, AppRoles.Admin);


            }

        }
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
