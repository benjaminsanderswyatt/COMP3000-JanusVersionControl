using System;
using backend.Models;
using backend.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace backend.Services
{
    public class UserService
    {
        private readonly JanusDbContext _context;

        public UserService(JanusDbContext context)
        {
            _context = context;
        }

        public async Task RegisterUserAsync(string username, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new Exception("Email has already been taken");

            var (passwordHash, salt) = PasswordSecurity.HashSaltPassword(password);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Salt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
