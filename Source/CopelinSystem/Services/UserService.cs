using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CopelinSystem.Models;

namespace CopelinSystem.Services
{
    public class UserService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly PasswordHasher _passwordHasher;

        public UserService(IDbContextFactory<ApplicationDbContext> contextFactory, PasswordHasher passwordHasher)
        {
            _contextFactory = contextFactory;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        public async Task<List<User>> GetAllUsers()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .OrderBy(u => u.Firstname)
                .ThenBy(u => u.Lastname)
                .ToListAsync();
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<User?> GetUserById(int userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.FindAsync(userId);
        }

        /// <summary>
        /// Update existing user
        /// </summary>
        public async Task<bool> UpdateUser(User user, string? rawPassword = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Check if user exists
            var existingUser = await context.Users.FindAsync(user.UserId);
            if (existingUser == null) return false;

            // Update fields
            existingUser.Firstname = user.Firstname;
            existingUser.Lastname = user.Lastname;
            existingUser.Email = user.Email;
            existingUser.Region = user.Region;
            existingUser.UserType = user.UserType;
            existingUser.AdUsername = user.AdUsername;
            existingUser.AdDomain = user.AdDomain;
            existingUser.AdSid = user.AdSid;
            
            // Only update password if a new one was provided
            if (!string.IsNullOrWhiteSpace(rawPassword))
            {
                existingUser.PasswordHash = _passwordHasher.HashPassword(rawPassword);
            }

            // Note: We don't update DateCreated or LastActive here usually

            context.Users.Update(existingUser);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<User> CreateUser(User user, string? rawPassword = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            user.DateCreated = DateTime.Now;
            user.LastActive = DateTime.Now;

            // Hash initial password if provided
            if (!string.IsNullOrWhiteSpace(rawPassword))
            {
                user.PasswordHash = _passwordHasher.HashPassword(rawPassword);
            }

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Delete user (or deactivate if preferred, but this deletes)
        /// </summary>
        public async Task<bool> DeleteUser(int userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Users.FindAsync(userId);
            if (user == null) return false;

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
