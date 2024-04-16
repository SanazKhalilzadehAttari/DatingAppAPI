using AutoMapper;
using DatingAppAPI.DTOs;
using DatingAppAPI.Entities;
using DatingAppAPI.Extnsions;
using DatingAppAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace DatingAppAPI.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public MessageHub(IUnitOfWork uow,
            IMapper mapper, IHubContext<PresenceHub> presenceHub)
        {
            _uow = uow;
            _mapper = mapper;
            _presenceHub = presenceHub;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUsers = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.getUser(), otherUsers);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
            var messages = await _uow.MessageRepository.GetMessageThread(Context.User.getUser(), otherUsers);
            if(_uow.HasChanges())
            {
                await _uow.Complete();
            }
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);

        }
        public async Task SendMessages(CreateMessageDTO createMessageDTO)
        {
            var userName = Context.User.getUser();
            if (userName == createMessageDTO.RecepientUserName)
            {
                throw new HubException("You cannot send messages to yourself");
            }

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(userName);
            var recepient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecepientUserName);
            if (recepient == null) {  throw new HubException("Not Found User"); }
            var message = new Message
            {
                Sender = sender,
                Recipient = recepient,
                SenderUsername = sender.UserName,
                RecipientUsername = recepient.UserName,
                Content = createMessageDTO.Content
            };
            var groupName = GetGroupName(sender.UserName,recepient.UserName);
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);
            if(group.connections.Any(x=> x.Username == recepient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recepient.UserName);
                if(connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new {username = sender.UserName,knownAs = sender.KnownAs});
                }
            }
            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage",_mapper.Map<MessageDto>(message));
            }
           

        }
        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.getUser());
            if(group == null)
            {
                group = new Group(groupName);
                _uow.MessageRepository.AddGroup(group);
            }

            group.connections.Add(connection);
            if (await _uow.Complete())
                return group;

            throw new HubException("Failed to add to group");
        }
        private async Task<Group> RemoveFromGroup()
        {
            var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.connections.FirstOrDefault(x=> x.ConnectionId==Context.ConnectionId);    
            _uow.MessageRepository.RemoveConnection(connection);
           if( await _uow.Complete()) return group;
            throw new HubException("Failed to remove from Group");
        }
    }
}
