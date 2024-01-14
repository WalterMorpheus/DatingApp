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
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public MessageController(IMapper mapper,IUnitOfWork uow)
    {
        _mapper = mapper;
        _uow = uow;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();
        if(username == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You can't send messages to yourself");

        var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
        if(recipient == null) return NotFound();

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };
        _uow.MessageRepostory.AddMessage(message);
        if(await _uow.Complete()) return Ok(_mapper.Map<MessageDto>(message));
        return BadRequest("Failed to send message");

    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.UserName = User.GetUsername();
        var messages = await _uow.MessageRepostory.GetMessagesForUsers(messageParams);

        Response.AddPaginationHeader(new PaginationHeader (messages.CurrentPage,messages.PageSize,
            messages.TotalCount, messages.TotalPages));

        return messages;
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var currentUsername = User.GetUsername();
        var message = await _uow.MessageRepostory.GetMessage(id);

        if(message.SenderUsername != currentUsername && message.RecipientUsername != currentUsername) 
            return Unauthorized();
        
        if (message.SenderUsername == currentUsername) message.SenderDeleted = true;
        if (message.RecipientUsername == currentUsername) message.RecipientDeleted = true;

        if(message.SenderDeleted && message.RecipientDeleted)
        {
            _uow.MessageRepostory.DeleteMessage(message);
        }

        if(await _uow.Complete()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}
