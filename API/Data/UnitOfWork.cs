using API.Interfaces;

namespace API.Data;

public class UnitOfWork(DataContext context, IUserRepository userRepository, ILikesRepository likesRepository, 
    IMessagesRepository messagesRepository, IPhotoRepository photoRepository) : IUnitOfWork
{
    public IUserRepository UserRepository => userRepository;

    public ILikesRepository LikesRepository => likesRepository;

    public IMessagesRepository MessagesRepository => messagesRepository;

    public IPhotoRepository PhotoRepository => photoRepository;

    public async Task<bool> CompleteAsync()
    {
        var result = await context.SaveChangesAsync();
        return result > 0;
    }

    public bool HasChanges()
    {
        var result = context.ChangeTracker.HasChanges();
        return result;
    }
}
