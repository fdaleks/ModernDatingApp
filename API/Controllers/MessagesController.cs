using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers.Params;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IMessagesRepository messagesRepository, IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(MessageCreateDto messageCreateDto)
    {
        var userName = User.GetUserName();
        if (userName == messageCreateDto.RecipientUserName.ToLower()) return BadRequest("You can't message yourself");

        var sender = await userRepository.GetUserByNameAsync(userName);
        var recipient = await userRepository.GetUserByNameAsync(messageCreateDto.RecipientUserName);

        if (sender == null || sender.UserName == null || recipient == null || recipient.UserName == null) 
            return BadRequest("Can't send a message");

        var message = new Message
        {
            Sender = sender,
            SenderUserName = sender.UserName,
            Recipient = recipient,
            RecipientUserName = recipient.UserName,
            Content = messageCreateDto.Content,
        };

        messagesRepository.AddMessage(message);

        if (await messagesRepository.SaveAllAsync())
            return Ok(mapper.Map<MessageDto>(message));

        return BadRequest("Failed to send a message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.UserName = User.GetUserName();

        var messages = await messagesRepository.GetMessagesForUserAsync(messageParams);

        Response.AddPaginationHeader(messages);

        return Ok(messages);
    }

    [HttpGet("thread/{userName}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string userName)
    {
        var currentUserName = User.GetUserName();

        var messages = await messagesRepository.GetMessageThreadAsync(currentUserName, userName);

        return Ok(messages);
    }

    [HttpDelete("{messageId:int}")]
    public async Task<ActionResult> DeleteMessage(int messageId)
    {
        var currentUserName = User.GetUserName();

        var messages = await messagesRepository.GetMessageByIsAsync(messageId);
        if (messages == null) return BadRequest("Can't delete this message");

        if (messages.SenderUserName != currentUserName && messages.RecipientUserName != currentUserName)
            return Forbid();

        if (messages.SenderUserName == currentUserName) messages.SenderDeleted = true;
        if (messages.RecipientUserName == currentUserName) messages.RecipientDeleted = true;

        if (messages is { SenderDeleted: true, RecipientDeleted: true })
        {
            messagesRepository.DeleteMessage(messages);
        }

        if (await messagesRepository.SaveAllAsync()) return Ok();
        return BadRequest("Failed to delete a message");
    }
}
