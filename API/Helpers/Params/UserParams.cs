﻿namespace API.Helpers.Params;

public class UserParams : PaginationParams
{
    public string? CurrentUsername { get; set; }

    public int MinAge { get; set; } = 18;

    public int MaxAge { get; set; } = 60;

    public string? Gender { get; set; }

    public string OrderBy { get; set; } = "lastActive";
}
