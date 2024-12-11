namespace API.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    ILikesRepository LikesRepository { get; }
    IMessagesRepository MessagesRepository { get; }
    IPhotoRepository PhotoRepository { get; }    

    Task<bool> CompleteAsync();
    bool HasChanges();
}
