using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Hubs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/[Controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHubContext<MessageHub> _hubContext;

        public MessagesController(IDatingRepository repo, 
            IMapper mapper,
            IHubContext<MessageHub> hubContext)
        {
            _mapper = mapper;
            _hubContext = hubContext;
            _repo = repo;
        }

        [HttpGet("{id}",Name="GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _repo.GetMessage(id);

            if(messageFromRepo == null)
            {
                return NotFound();
            }

            return Ok(messageFromRepo);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessagesThread(int userId, int recipientId)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _repo.GetMessagesThread(userId, recipientId);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);

            return Ok(messageThread);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserMessages(int userId, [FromQuery]MessageParams messageParams)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageParams.UserId=userId;
            
            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, 
                messagesFromRepo.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPage);
            return Ok(messages);
        }
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repo.GetUser(userId);

            if(sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageForCreationDto.SenderId = userId;

            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

            if(recipient == null)
            {
                return BadRequest("Could not find user");
            }

            var message = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add(message);


            if(await _repo.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new{userId=userId,id = message.Id}, messageToReturn);
            }

            throw new Exception("Creating the message failed on save");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _repo.GetMessage(id);
            
            if(messageFromRepo.SenderId == userId)
            {
                messageFromRepo.SenderDeleted = true;
            }

            if(messageFromRepo.RecipientId == userId)
            {
                messageFromRepo.RecipientDeleted = true;
            }

            if(messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
            {
                _repo.Delete(messageFromRepo);
            }
            
            if(await _repo.SaveAll())
            {
                return NoContent();
            }

            throw new Exception("Error deleting the message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int id, int userId)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var message = await _repo.GetMessage(id);

            if(message.RecipientId != id)
            {
                return Unauthorized();
            }

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _repo.SaveAll();
            
            return NoContent();
        }
    }
}