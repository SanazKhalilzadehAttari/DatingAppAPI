using AutoMapper;
using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Extnsions;
using DatingAppAPI.Helpers;
using DatingAppAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DatingAppAPI.Controllers
{
    public class MessagesController : APIBaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public MessagesController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;   
        }
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var userName = User.getUser();
            if (userName == createMessageDTO.RecepientUserName.ToLower())
            {
                return BadRequest("You cannot send a message to yourself");
            }
            
            var sender = await _uow.UserRepository.GetUserByUsernameAsync(userName);
            var recepient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecepientUserName);
            if (recepient == null) { return NotFound(); }
            var message = new Message
            {
                Sender = sender,
                Recipient = recepient,
                SenderUsername = sender.UserName,
                RecipientUsername = recepient.UserName,
                Content = createMessageDTO.Content
            };
            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }
            return BadRequest("Failed to send Message");

        }



        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessageFromUser([FromQuery] MessageParams messageparams)
        {

            messageparams.Username = User.getUser();
            var messages = await _uow.MessageRepository.GetMessageForUser(messageparams);
            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize,
                messages.TotalCount, messages.TotalPages));

            return messages;

        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUserName = User.getUser();
            return Ok(await _uow.MessageRepository.GetMessageThread(currentUserName, username));
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.getUser();
            var message = await _uow.MessageRepository.GetMessage(id);
            if (message.SenderUsername != username && message.RecipientUsername != username)
            {
                return Unauthorized();
            }
            if (message.RecipientUsername == username) { message.RecipientDeleted = true; }
            if (message.SenderUsername == username) { message.SenderDeleted = true; }

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _uow.MessageRepository.DeleteMessage(message);

            }
            if (await _uow.Complete()) return Ok();
            return BadRequest("there is an Error in deleting happend");
        }

       

    }
}
