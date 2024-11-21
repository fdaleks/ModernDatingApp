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
public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(MessageCreateDto dto)
    {
        var userName = User.GetUserName();
        if (userName == dto.RecipientUserName.ToLower()) return BadRequest("You can't message yourself");

        var sender = await unitOfWork.UserRepository.GetUserByNameAsync(userName);
        var recipient = await unitOfWork.UserRepository.GetUserByNameAsync(dto.RecipientUserName);

        if (sender == null || sender.UserName == null || recipient == null || recipient.UserName == null) 
            return BadRequest("Can't send a message");

        var message = new Message
        {
            Sender = sender,
            SenderUserName = sender.UserName,
            Recipient = recipient,
            RecipientUserName = recipient.UserName,
            Content = dto.Content,
        };

        unitOfWork.MessagesRepository.AddMessage(message);

        if (await unitOfWork.CompleteAsync())
            return Ok(mapper.Map<MessageDto>(message));

        return BadRequest("Failed to send a message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.UserName = User.GetUserName();

        var messages = await unitOfWork.MessagesRepository.GetMessagesForUserAsync(messageParams);

        Response.AddPaginationHeader(messages);

        return Ok(messages);
    }
    
    [HttpGet("thread/{userName}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string userName)
    {
        var currentUserName = User.GetUserName();

        var messages = await unitOfWork.MessagesRepository.GetMessageThreadAsync(currentUserName, userName);

        return Ok(messages);
    }
    
    [HttpDelete("{messageId:int}")]
    public async Task<ActionResult> DeleteMessage(int messageId)
    {
        var currentUserName = User.GetUserName();

        var messages = await unitOfWork.MessagesRepository.GetMessageByIsAsync(messageId);
        if (messages == null) return BadRequest("Can't delete this message");

        if (messages.SenderUserName != currentUserName && messages.RecipientUserName != currentUserName)
            return Forbid();

        if (messages.SenderUserName == currentUserName) messages.SenderDeleted = true;
        if (messages.RecipientUserName == currentUserName) messages.RecipientDeleted = true;

        if (messages is { SenderDeleted: true, RecipientDeleted: true })
        {
            unitOfWork.MessagesRepository.DeleteMessage(messages);
        }

        if (await unitOfWork.CompleteAsync()) return Ok();
        return BadRequest("Failed to delete a message");
    }
}
