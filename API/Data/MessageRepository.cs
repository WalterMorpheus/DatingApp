using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository : IMessageRepostory
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.Messages.Remove(message);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _context.Groups
            .Include(x=> x.Connections)
            .Where(x=> x.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _context.Groups
            .Include(x=> x.Connections)
            .FirstOrDefaultAsync(x=> x.Name == groupName);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUsers(MessageParams messageParams)
    {
        var query = _context.Messages
            .OrderByDescending(x=>x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox"=> query.Where(x=>x.RecipientUsername == messageParams.UserName 
                && x.RecipientDeleted == false),
            "Outbox"=> query.Where(x=>x.SenderUsername == messageParams.UserName 
                && x.SenderDeleted == false),
            _ => query.Where(x=>x.RecipientUsername == messageParams.UserName 
                && x.RecipientDeleted == false && x.DateRead == null)
        };

        var message = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
        return await PagedList<MessageDto>.CreateAsync(message,messageParams.PageNumber,messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageTread(string currentUsername,string recipientUsername)
    {
        var query =  _context.Messages
            .Where(
                x=> x.RecipientUsername == currentUsername && x.RecipientDeleted == false &&
                x.SenderUsername == recipientUsername ||
                x.RecipientUsername == recipientUsername && x.SenderDeleted == false &&
                x.SenderUsername == currentUsername 
            )
            .OrderBy(x=>x.MessageSent)
            .AsQueryable();

        var unreadMessages = query.Where(x=>x.DateRead == null &&
            x.RecipientUsername == currentUsername).ToList();
        
        if(unreadMessages.Any())
        {
            foreach(var message in unreadMessages)
            {
                message.DateRead = DateTime.UtcNow;
            }
        }
        return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }
}
