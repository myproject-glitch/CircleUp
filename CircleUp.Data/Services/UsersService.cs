using CircleUp.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleUp.Data.Services
{
    public class UsersService:IUsersService
    {
        private readonly AppDbContext _appDbContext;

        public UsersService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<User> GetUser(int loggedInUserid)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(n => n.Id == loggedInUserid)
                ?? new User();
        }

        public async Task UpdateUserProfilePicture(int loggedInUserId, string profilePictureUrl)
        {
            var userDb = await _appDbContext.Users.FirstOrDefaultAsync(n => n.Id == loggedInUserId);
            if (userDb != null) 
            {
                userDb.ProfilePictureUrl = profilePictureUrl;
                _appDbContext.Users.Update(userDb);
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
}
