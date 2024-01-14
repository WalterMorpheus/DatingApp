namespace API.Interfaces;
/*A unit of work is like a transation block*/
public interface IUnitOfWork
{
    IUserRepository UserRepository {get;}
    IMessageRepostory MessageRepostory {get;}
    ILikesRepository LikesRepository {get;}
    Task<bool> Complete();
    bool HasChanges();
}
