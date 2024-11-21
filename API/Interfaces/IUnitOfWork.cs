namespace API.Interfaces;

public interface IUnitOfWork
{
    ILikesRepository LikesRepository { get; }
    IMessagesRepository MessagesRepository { get; }
    IUserRepository UserRepository { get; }

    Task<bool> CompleteAsync();
    bool HasChanges();
}
