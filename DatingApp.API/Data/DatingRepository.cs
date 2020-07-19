using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context=context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes
                .FirstOrDefaultAsync(e=>e.LikerId==userId && e.LikeeId==recipientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(e=>e.UserId == userId).FirstOrDefaultAsync(p=>p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(e=>e.Photos).FirstOrDefaultAsync(e=>e.Id==id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p=>p.Photos).OrderByDescending(e=>e.LastActive).AsQueryable();

            users = users.Where(e=>e.Id != userParams.UserId);

            //users = users.Where(e=>e.Gender == userParams.Gender);

            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId,userParams.Likers);
                users = users.Where(e=>userLikers.Contains(e.Id));
            }

            if(userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId,userParams.Likers);
                users = users.Where(e=>userLikees.Contains(e.Id));
            }

            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDOB = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDOB = DateTime.Today.AddYears(-userParams.MinAge);
                
                users = users.Where(e=>e.DateOfBirth >= minDOB && e.DateOfBirth <=maxDOB);
            }
            
            if(!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(e=>e.Created);
                        break;
                    default:
                        users = users.OrderByDescending(e=>e.LastActive);
                        break;
                }
            }
            return await PagedList<User>.CreateAsync(users,userParams.PageNumber,userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                .Include(x=>x.Likers)
                .Include(x=>x.Likees)
                .FirstOrDefaultAsync(e=>e.Id==id);

            if(likers)
            {
                return user.Likers.Where(e=>e.LikeeId==id).Select(i=>i.LikerId);
            }
            else
            {
                return user.Likees.Where(e=>e.LikerId==id).Select(i=>i.LikeeId);
            }
        }
        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync()>0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(e=>e.Id==id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages.Include(e=>e.Sender).ThenInclude(e=>e.Photos)
                            .Include(e=>e.Recipient).ThenInclude(e=>e.Photos)
                            .AsQueryable();
            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(e=>e.RecipientId == messageParams.UserId 
                        && e.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(e=>e.SenderId == messageParams.UserId 
                        && e.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(e=>e.RecipientId == messageParams.UserId &&
                         e.RecipientDeleted== false && e.IsRead==false);
                    break;
            }
            messages = messages.OrderByDescending(e=>e.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessagesThread(int userId, int recipientId)
        {
            var messages = await _context.Messages.Include(e=>e.Sender).ThenInclude(e=>e.Photos)
                            .Include(e=>e.Recipient).ThenInclude(e=>e.Photos)
                            .Where(e=>e.RecipientId==userId && e.RecipientDeleted == false && e.SenderId==recipientId 
                                || e.RecipientId == recipientId && e.SenderId==userId && e.SenderDeleted == false)
                            .OrderByDescending(e=>e.MessageSent)
                            .ToListAsync();

            return messages;
        }
    }
}