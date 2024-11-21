using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController(DataContext context) : BaseApiController
{
    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetAuth()
    {
        return "secret text";
    }

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        var result = context.Users.Find(-1);
        if (result == null)
        {
            return NotFound();
        }
        else
        {
            return result;
        }
    }

    [HttpGet("server-error")]
    public ActionResult<AppUser> GetServerError()
    {
        var result = context.Users.Find(-1) ?? throw new Exception("A bad thing had happened.");

        return result;
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
        return BadRequest("This is not a good request.");
    }
}
