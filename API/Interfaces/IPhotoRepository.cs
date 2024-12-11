using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    Task<IEnumerable<PhotoForModerationDto>> GetPhotosForModerationAsync();

    Task<Photo?> GetPhotoByIdAsync(int photoId);

    void RemovePhoto(Photo photo);
}
