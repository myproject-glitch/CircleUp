using CircleUp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircleUp.Data.Services
{
    public interface IUsersService
    {
        Task<User> GetUser(int loggedInUser);
        Task UpdateUserProfilePicture(int loggedInUserId, string profilePictureUrl);
    }
}
