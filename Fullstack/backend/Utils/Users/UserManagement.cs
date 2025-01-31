using backend.Auth;
using backend.DataTransferObjects;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Utils.Users
{
    public class UserManagement
    {
        private readonly JanusDbContext _janusDbContext;

        public UserManagement(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }



        // Get user by Id
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _janusDbContext.Users.FindAsync(userId);
        }


        // Get user by username
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }


        // Get user by email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }


        // Add a new user
        public async Task<ReturnObject> AddUserAsync(RegisterUserDto newUser)
        {
            if (await _janusDbContext.Users.AnyAsync(u => u.Username == newUser.Username))
                return new ReturnObject { Success = false, Message = "Username has already been taken" };

            if (await _janusDbContext.Users.AnyAsync(u => u.Email == newUser.Email))
                return new ReturnObject { Success = false, Message = "Email has already been taken" };

            try
            {

                var (passwordHash, salt) = PasswordSecurity.HashSaltPassword(newUser.Password);

                var user = new User
                {
                    Username = newUser.Username,
                    Email = newUser.Email,
                    PasswordHash = passwordHash,
                    Salt = salt
                };


                await _janusDbContext.Users.AddAsync(user);
                await _janusDbContext.SaveChangesAsync();


                return new ReturnObject { Success = true, Message = "User registered successfully" };
            } 
            catch
            {
                return new ReturnObject { Success = false, Message = "Failed to register user" };
            }
            
        }


        // Delete user by Id
        public async Task<ReturnObject> DeleteUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);

            if (user == null)
                return new ReturnObject { Success = false, Message = "User not found" };

            try
            {
                _janusDbContext.Users.Remove(user);

                await _janusDbContext.SaveChangesAsync();

                return new ReturnObject { Success = true, Message = "User deleted successfully" };
            } 
            catch
            {
                return new ReturnObject { Success = false, Message = "Failed to delete user" };
            }
            
        }



        // Update email
        public async Task<ReturnObject> UpdateEmailAsync(int userId, string newEmail)
        {
            if (await _janusDbContext.Users.AnyAsync(u => u.Email == newEmail))
                return new ReturnObject { Success = false, Message = "Email is already in use" };

            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return new ReturnObject { Success = false, Message = "User not found" };

            try
            {
                user.Email = newEmail;

                await _janusDbContext.SaveChangesAsync();

                return new ReturnObject { Success = true, Message = "Email updated successfully" };
            }
            catch
            {
                return new ReturnObject { Success = false, Message = "Failed to update email" };
            }

        }


        // Update password
        public async Task<ReturnObject> UpdatePasswordAsync(int userId, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return new ReturnObject { Success = false, Message = "User not found" };


            try
            {
                var (passwordHash, salt) = PasswordSecurity.HashSaltPassword(newPassword);

                user.PasswordHash = passwordHash;
                user.Salt = salt;

                await _janusDbContext.SaveChangesAsync();
                return new ReturnObject { Success = true, Message = "Password updated successfully" };
            }
            catch
            {
                return new ReturnObject { Success = false, Message = "Failed to update password" };
            }
        }




    }
}
