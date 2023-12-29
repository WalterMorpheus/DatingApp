using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepostory
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message> GetMessage(int id);
    Task<PagedList<MessageDto>> GetMessagesForUsers(MessageParams messageParams);
    Task<IEnumerable<MessageDto>> GetMessageTread(string currentUsername,string RecipientUsername);
    Task<bool> SaveAllAsync();
}
