namespace API.DTOs;

public class MessageCreateDto
{
    public required string RecipientUserName { get; set; }
    public required string Content { get; set; }
}
