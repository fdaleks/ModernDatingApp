namespace API.DTOs;

public class PhotoForModerationDto
{
    public int Id { get; set; }
    public string? Url { get; set; }
    public string? UserName { get; set; }
    public bool IsModerated { get; set; }
    public bool IsApproved { get; set; }
}
