using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Helpers;
using DatingAppAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace DatingAppAPI.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages
            .OrderBy(x => x.DateSend)
            .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username &&
                 u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username &&
                    u.SenderDeleted == false),
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username
                    && u.RecipientDeleted == false && u.DateRead == null)
            };
            var messages1 = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).AsNoTracking();

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentuserName, string reciepientUserName)
        {
            var message = await _context.Messages.Include(x => x.Sender).ThenInclude(x => x.Photos)
                 .Include(x => x.Recipient).ThenInclude(x => x.Photos)
                 .Where(
                 m => m.Recipient.UserName == currentuserName&& m.RecipientDeleted == false && m.Sender.UserName == reciepientUserName
                 || m.Sender.UserName == currentuserName && m.Recipient.UserName == reciepientUserName && m.SenderDeleted==false
                 ).OrderBy(m => m.DateSend)
                 .ToListAsync();
            var unreadMessages = message.Where(m => m.DateRead == null &&
            m.RecipientUsername == currentuserName).ToList();
            if (unreadMessages.Any())
            {
                foreach (var m in unreadMessages)
                {
                    m.DateRead = DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(message);


        }
       

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
