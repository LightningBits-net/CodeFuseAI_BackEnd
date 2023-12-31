﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CodeFuseAI_Shared.Data;
using CodeFuseAI_Shared.Models;
using CodeFuseAI_Shared.Repository.IRepository;

namespace CodeFuseAI_Shared.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public MessageRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MessageDTO>> GetAllByConversationId(int conversationId)
        {
            try
            {
                var favMessages = await _db.Messages
                    .Where(m => m.ConversationId == conversationId && m.IsFav)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                var nonFavMessages = await _db.Messages
                    .Where(m => m.ConversationId == conversationId && !m.IsFav)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                var messages = favMessages.Concat(nonFavMessages).OrderBy(m => m.Timestamp);

                return _mapper.Map<IEnumerable<Message>, IEnumerable<MessageDTO>>(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MessageRepository.GetAllByConversationId: {ex.Message}");
                return null;
            }
        }

        public async Task<MessageDTO> Create(MessageDTO objDTO)
        {
            try
            {
                var obj = _mapper.Map<MessageDTO, Message>(objDTO);
                obj.Timestamp = DateTime.UtcNow;
                obj.IsUserMessage = objDTO.IsUserMessage;

                var addedObj = await _db.Messages.AddAsync(obj);
                await _db.SaveChangesAsync();

                var conversation = await _db.Conversations
                    .Include(c => c.Client)
                    .FirstOrDefaultAsync(c => c.Id == obj.ConversationId);

                var clientId = conversation?.ClientId ?? 0;

                return _mapper.Map<Message, MessageDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MessageRepository.Create: {ex.Message}");
                return null;
            }
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                var obj = await _db.Messages.FirstOrDefaultAsync(m => m.Id == id);
                if (obj != null)
                {
                    _db.Messages.Remove(obj);
                    return await _db.SaveChangesAsync();
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MessageRepository.Delete: {ex.Message}");
                return -1;
            }
        }

        public async Task<MessageDTO> Update(MessageDTO objDTO)
        {
            try
            {
                var objFromDb = await _db.Messages.FirstOrDefaultAsync(m => m.Id == objDTO.Id);
                if (objFromDb != null)
                {
                    objFromDb.Content = objDTO.Content;
                    objFromDb.Timestamp = objDTO.Timestamp;
                    objFromDb.IsUserMessage = objDTO.IsUserMessage;
                    objFromDb.ConversationId = objDTO.ConversationId;
                    objFromDb.IsFav = objDTO.IsFav;

                    _db.Messages.Update(objFromDb);
                    await _db.SaveChangesAsync();
                    return _mapper.Map<Message, MessageDTO>(objFromDb);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MessageRepository.Update: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ToggleFavorite(int id)
        {
            try
            {
                var message = await _db.Messages.FirstOrDefaultAsync(m => m.Id == id && !m.IsUserMessage);
                if (message != null)
                {
                    message.IsFav = !message.IsFav;
                    _db.Messages.Update(message);

                    await _db.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MessageRepository.ToggleFavorite: {ex.Message}");
                return false;
            }
        }

    }
}