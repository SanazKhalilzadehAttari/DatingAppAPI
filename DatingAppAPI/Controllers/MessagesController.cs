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
        private readonly IUserRepository _user;
        private readonly IMessageRepository _message;
        private readonly IMapper _mapper;
        public MessagesController(IUserRepository user, IMessageRepository message,
            IMapper mapper)
        {
            _user = user;
            _message = message;
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
            
            var sender = await _user.GetUserByUsernameAsync(userName);
            var recepient = await _user.GetUserByUsernameAsync(createMessageDTO.RecepientUserName);
            if (recepient == null) { return NotFound(); }
            var message = new Message
            {
                Sender = sender,
                Recipient = recepient,
                SenderUsername = sender.UserName,
                RecipientUsername = recepient.UserName,
                Content = createMessageDTO.Content
            };
            _message.AddMessage(message);

            if (await _message.SaveAllAsync())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }
            return BadRequest("Failed to send Message");

        }



        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessageFromUser([FromQuery] MessageParams messageparams)
        {

            messageparams.Username = User.getUser();
            var messages = await _message.GetMessageForUser(messageparams);
            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize,
                messages.TotalCount, messages.TotalPages));

            return messages;

        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUserName = User.getUser();
            return Ok(await _message.GetMessageThread(currentUserName, username));
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.getUser();
            var message = await _message.GetMessage(id);
            if (message.SenderUsername != username && message.RecipientUsername != username)
            {
                return Unauthorized();
            }
            if (message.RecipientUsername == username) { message.RecipientDeleted = true; }
            if (message.SenderUsername == username) { message.SenderDeleted = true; }

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _message.DeleteMessage(message);

            }
            if (await _message.SaveAllAsync()) return Ok();
            return BadRequest("there is an Error in deleting happend");
        }

    }
}
