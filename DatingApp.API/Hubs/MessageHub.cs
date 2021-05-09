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
        public async Task SendMessage(MessageForCreationDto messageDto, string connectionId)
        {
            if (messageDto.RecipientId != 0)
            {

                var recipient = await _repository.GetUser(messageDto.RecipientId);
                if (recipient != null)
                {
                    var message = _mapper.Map<Message>(messageDto);

                    _repository.Add(message);

                    if (await _repository.SaveAll())
                    {
                        await Clients.All.SendAsync("MessageReceived", messageDto);
                    }
                }
            }
        }

        public string getConnectionId() => Context.ConnectionId;
    }
}
