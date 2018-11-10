using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Network.API.Models;

namespace Network.API.Data
{
    public class NetworkRepository : INetworkRepository
    {
        private readonly DataContext _context;

        public NetworkRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var dbPhoto = await _context.Photos.SingleOrDefaultAsync(p => p.Id == id);

            return dbPhoto;
        }

        public async Task<User> GetUser(int id)
        {
            var dbUser = await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.Id == id);

            return dbUser;
        }

        public async Task<Photo> GetUserProfilePhoto(int userId)
        {
            return await _context.Photos.SingleOrDefaultAsync(p => p.UserId == userId && p.IsProfilePhoto == true);
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var dbUsers = await _context.Users.Include(p => p.Photos).ToListAsync();

            return dbUsers;
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
