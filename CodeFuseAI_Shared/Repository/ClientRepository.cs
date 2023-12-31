﻿// LightningBits
using System;
using CodeFuseAI_Shared.Repository.IRepository;
using AutoMapper;
using CodeFuseAI_Shared.Data;
using CodeFuseAI_Shared;
using Microsoft.EntityFrameworkCore;
using CodeFuseAI_Shared.Models;
using System.Reflection.Metadata;

namespace CodeFuseAI_Shared.Repository
{
    public class ClientRepository : IClientRepository
    {

        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public ClientRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ClientDTO> Create(ClientDTO objDTO)
        {
            var obj = _mapper.Map<ClientDTO, Client>(objDTO);
            obj.DateCreated = DateTime.Now;
            obj.BillingAmount ??= 0; // Assign 0 if BillingAmount is null
            obj.BillingCycle ??= string.Empty; // Assign an empty string if BillingCycle is null
            obj.BillingEndDate ??= DateTime.MinValue; // Assign DateTime.MinValue if BillingEndDate is null
            obj.BillingStartDate ??= DateTime.MinValue; // Assign DateTime.MinValue if BillingStartDate is null
            obj.IsActive ??= false; // Assign false if IsActive is null
            obj.UserId ??= string.Empty; // Assign an empty string if UserId is null

            var addedObj = _db.Clients.Add(obj);
            await _db.SaveChangesAsync();

            return _mapper.Map<Client, ClientDTO>(addedObj.Entity);
        }


        public async Task<int> Delete(int id)
        {
            var obj = await _db.Clients.FirstOrDefaultAsync(u => u.ClientId == id);
            if (obj != null)
            {
                _db.Clients.Remove(obj);
                return await _db.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<ClientDTO> Get(int id)
        {
            var client = await _db.Clients.FirstOrDefaultAsync(u => u.ClientId == id);
            if (client != null)
            {
                return _mapper.Map<Client, ClientDTO>(client);
            }
            return new ClientDTO();
        }


        public Task<IEnumerable<ClientDTO>> GetAll()
        {
            return Task.FromResult(_mapper.Map<IEnumerable<Client>, IEnumerable<ClientDTO>>(_db.Clients));
        }

        public async Task<ClientDTO> Update(ClientDTO objDTO)
        {
            var objFromDb = await _db.Clients.FirstOrDefaultAsync(u => u.ClientId == objDTO.ClientId);
            if (objFromDb != null)
            {
                objFromDb.Name = objDTO.Name;
                objFromDb.Address = objDTO.Address;
                objFromDb.DomainName = objDTO.DomainName;
                objFromDb.DateCreated = objDTO.DateCreated;
                objFromDb.Description = objDTO.Description;
                objFromDb.Email = objDTO.Email;
                objFromDb.Counter = objDTO.Counter;
                objFromDb.ImageUrl = objDTO.ImageUrl;
                objFromDb.IsActive = objDTO.IsActive;
                objFromDb.UserId = objDTO.UserId;
                objFromDb.BillingCycle = objDTO.BillingCycle;
                objFromDb.BillingAmount = objDTO.BillingAmount;
                objFromDb.BillingStartDate = objDTO.BillingStartDate;
                objFromDb.BillingEndDate = objDTO.BillingEndDate;
                _db.Clients.Update(objFromDb);
                await _db.SaveChangesAsync();
                return _mapper.Map<Client, ClientDTO>(objFromDb);
            }
            return objDTO;
        }

        public async Task<ClientFrontendDTO> GetClientFrontendData(int clientId)
        {
            var client = await _db.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
            if (client != null)
            {
                return _mapper.Map<Client, ClientFrontendDTO>(client);
            }
            return null;
        }
    }
}

