﻿using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public string? Gender { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string? KnownAs { get; set; }

    [Required]
    public string? DateOfBirth { get; set; }

    [Required]
    public string? City { get; set; }

    [Required]
    public string? Country { get; set; }

    [Required]
    [MinLength(4), MaxLength(10)]
    public string Password { get; set; } = string.Empty;
}
