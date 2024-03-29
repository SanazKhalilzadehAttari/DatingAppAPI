﻿using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Helpers;

namespace DatingAppAPI.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int id);
        Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string  currentUserName, string reciepientUserName);

        Task<bool> SaveAllAsync();

    }
}
