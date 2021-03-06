﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Network.API.Helpers;
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
            var dbPhoto = await _context.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.Id == id);

            return dbPhoto;
        }

        public async Task<User> GetUser(int id, bool isCurrentUser)
        {
            var query = _context.Users.Include(p => p.Photos).AsQueryable();

            if (isCurrentUser)
                query = query.IgnoreQueryFilters();

            var dbUser = await query.SingleOrDefaultAsync(u => u.Id == id);

            return dbUser;
        }

        public async Task<Photo> GetUserProfilePhoto(int userId)
        {
            return await _context.Photos.SingleOrDefaultAsync(p => p.UserId == userId && p.IsProfilePhoto == true);
        }

        public async Task<Friend> GetFriend(int userId, int friendId)
        {
            return await _context.Friends.SingleOrDefaultAsync(x => x.FrienderId == userId && x.FriendeeId == friendId);
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var dbUsers = _context.Users.Include(p => p.Photos).Where(u => u.Id != userParams.UserId).OrderByDescending(u => u.LastActive).AsQueryable();

            if(!string.IsNullOrEmpty(userParams.Gender))
                dbUsers = dbUsers.Where(u => u.Gender == userParams.Gender);

            if (userParams.Friendees)
            {
                var userFriendees = await GetUserFriends(userParams.UserId, userParams.Friendees);
                dbUsers = dbUsers.Where(x => userFriendees.Contains(x.Id));
            }

            if (userParams.Frienders)
            {
                var userFrienders = await GetUserFriends(userParams.UserId, userParams.Friendees);
                dbUsers = dbUsers.Where(x => userFrienders.Contains(x.Id));
            }

            if(userParams.MinAge != 1 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                dbUsers = dbUsers.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                if (userParams.OrderBy == "created")
                    dbUsers = dbUsers.OrderByDescending(u => u.Created);
                else
                    dbUsers = dbUsers.OrderByDescending(u => u.LastActive);
            }

            return await PagedList<User>.CreateAsync(dbUsers, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task<IEnumerable<int>> GetUserFriends(int id, bool friendees)
        {
            var currentUser = await _context.Users.Include(u => u.Friendees).Include(u => u.Frienders).SingleOrDefaultAsync(u => u.Id == id);

            if (friendees)
            {
                return currentUser.Friendees.Where(f => f.FrienderId == id).Select(i => i.FriendeeId);
            }
            else
            {
                return currentUser.Frienders.Where(f => f.FriendeeId == id).Select(i => i.FrienderId);
            }
        }
    }
}
