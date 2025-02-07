﻿using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser<int>
{
    public DateOnly DateOfBirth { get; set; }
    public required string KnownAs { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public required string Gender { get; set; }
    public string? Introduction { get; set; }
    public string? Interests { get; set; }
    public string? LookingFor { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    // photos
    public List<Photo> Photos { get; set; } = [];
    // likes
    public List<UserLike> CurrentUserLikedBy { get; set; } = [];
    public List<UserLike> LikedByCurrentUser { get; set; } = [];
    // messages
    public List<Message> MessagesSent { get; set; } = [];
    public List<Message> MessagesReceived { get; set; } = [];
    //roles
    public ICollection<AppUserRole> UserRoles { get; set; } = [];
}
