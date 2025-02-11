using backend.Auth;
using backend.DataTransferObjects;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Utils.Users
{
    public class ProfilePicManagement
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly UserManagement _userManagement;

        public ProfilePicManagement(JanusDbContext janusDbContext, UserManagement userManagement)
        {
            _janusDbContext = janusDbContext;
            _userManagement = userManagement;
        }

        // Get profile picture path
        public async Task<string> GetProfilePicturePathAsync(int userId)
        {
            var user = await _userManagement.GetUserByIdAsync(userId);
            if (user == null)
                return null;

            return user.ProfilePicturePath;
        }




        // Update profile picture path
        public async Task<ReturnObject> SaveProfilePicturePathAsync(int userId, string imagePath)
        {
            var user = await _userManagement.GetUserByIdAsync(userId);
            if (user == null)
                return new ReturnObject { Success = false, Message = "User not found" };

            try
            {
                user.ProfilePicturePath = imagePath;

                await _janusDbContext.SaveChangesAsync();

                return new ReturnObject { Success = true, Message = "Profile picture updated successfully" };
            }
            catch
            {
                return new ReturnObject { Success = false, Message = "Failed to update profile picture" };
            }

        }





    }
}
