using API.Interfaces;

namespace API.Data;

public class UnitOfWork(DataContext context, ILikesRepository likesRepository, 
    IMessagesRepository messagesRepository, IUserRepository userRepository) : IUnitOfWork
{
    public ILikesRepository LikesRepository => likesRepository;

    public IMessagesRepository MessagesRepository => messagesRepository;

    public IUserRepository UserRepository => userRepository;

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
