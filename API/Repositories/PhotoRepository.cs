using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
{
    public async Task<IEnumerable<PhotoForModerationDto>> GetPhotosForModerationAsync()
    {
        var result = await context.Photos
            .IgnoreQueryFilters()
            .Where(x => !x.IsModerated)
            .ProjectTo<PhotoForModerationDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return result;
    }

    public async Task<Photo?> GetPhotoByIdAsync(int photoId)
    {
        var result = await context.Photos
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == photoId);

        return result;
    }

    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}
