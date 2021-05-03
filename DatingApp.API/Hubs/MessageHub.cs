using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Hubs
{
    public class MessageHub:Hub
    {

        private readonly static ConnectionMappings<string> _connections = new ConnectionMappings<string>();
        private readonly IDatingRepository _repository;
        private readonly IMapper _mapper;


        public MessageHub(IDatingRepository repository,IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task SendMessage(Message messageDto)
        {
            await Clients.All.SendAsync("MessageReceived", messageDto);

            //List<string> receiverIds = _connections.GetConnections(messageDto.RecipientId.ToString()).ToList<string>();
            //if (receiverIds.Count > 0)
            //{

            //    var recipient = await _repository.GetUser(messageDto.RecipientId);
            //    if (recipient != null)
            //    {
            //        var message = _mapper.Map<Message>(messageDto);

            //        _repository.Add(message);

            //        if(await _repository.SaveAll())
            //        {
            //            await Clients.Clients(receiverIds).SendAsync("MessageReceived", messageDto);
            //        }
            //    }
            //}
        }
    }
}
