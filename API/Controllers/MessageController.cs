using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessageController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepostory _messageRepostory;
    private readonly IMapper _mapper;

    public MessageController(IUserRepository userRepository,IMessageRepostory messageRepostory,IMapper mapper)
    {
        _userRepository = userRepository;
        _messageRepostory = messageRepostory;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();
        if(username == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You can't send messages to yourself");

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
        if(recipient == null) return NotFound();

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };
        _messageRepostory.AddMessage(message);
        if(await _messageRepostory.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));
        return BadRequest("Failed to send message");

    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.UserName = User.GetUsername();
        var messages = await _messageRepostory.GetMessagesForUsers(messageParams);

        Response.AddPaginationHeader(new PaginationHeader (messages.CurrentPage,messages.PageSize,
            messages.TotalCount, messages.TotalPages));

        return messages;
    }
    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();
        return Ok(await _messageRepostory.GetMessageTread(currentUsername,username));
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var currentUsername = User.GetUsername();
        var message = await _messageRepostory.GetMessage(id);

        if(message.SenderUsername != currentUsername && message.RecipientUsername != currentUsername) 
            return Unauthorized();
        
        if (message.SenderUsername == currentUsername) message.SenderDeleted = true;
        if (message.RecipientUsername == currentUsername) message.RecipientDeleted = true;

        if(message.SenderDeleted && message.RecipientDeleted)
        {
            _messageRepostory.DeleteMessage(message);
        }

        if(await _messageRepostory.SaveAllAsync()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}
