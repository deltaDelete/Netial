using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netial.Database;
using Netial.Database.Models;

namespace Netial.Api; 

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase {
    private static Regex _matchHtmlTags = new Regex("<[^>]*>");
    private readonly ILogger<PostsController> _logger;
    private readonly ApplicationContext _db;

    public CommentsController(ILogger<PostsController> logger, ApplicationContext db) {
        _logger = logger;
        _db = db;
    }

    [HttpPost("/api/posts/{postId:guid}/comments")]
    public async Task<IActionResult> NewComment([FromRoute] Guid postId, [FromForm] string text) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) {
            return Unauthorized();
        }

        if (!Guid.TryParse(userGuid, out Guid authorId)) {
            return Unauthorized();
        }
        
        var user = await _db.Users.FindAsync(authorId);
        if (user is null) {
            return NotFound($"Не найден пользователь с идентификатором {authorId}");
        }

        var post = await _db.Posts.FindAsync(postId);
        if (post is null) {
            return NotFound($"Не найден пост с идентификатором {postId}");
        }

        await _db.Comments.AddAsync(new Comment() {
            Author = user,
            Text = _matchHtmlTags.Replace(text, ""),
            Post = post
        });

        await _db.SaveChangesAsync();
        
        return Ok();
    }

    [HttpPost("comments/{id:guid}/like")]
    public async Task<IActionResult> LikeComment([FromRoute] Guid id) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) {
            return Unauthorized();
        }

        var comment = await _db.Comments.FindAsync(id);
        if (comment is null) {
            return NotFound(id);
        }
        
        var users = _db.Users
            .Include(u => u.LikedComments);
            
        var user = users.First(u => u.Id == Guid.Parse(userGuid));

        if (user.LikedComments.Any(x => x == comment)) {
            user.LikedComments.Remove(comment);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Комментарий {id} удален из понравившихся пользователя {userGuid}");
            return Ok($"Комментарий {id} успешно удален из понравившихся");
        }

        user.LikedComments.Add(comment);
        await _db.SaveChangesAsync();
        _logger.LogInformation($"Комментарий {id} добавлен в понравившиеся пользователя {userGuid}");
        return Ok($"Комментарий {id} успешно добавлен в понравившиеся");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetComment([FromRoute] Guid id) {
        Comment? comment = await _db.Comments
            .Include(x => x.Author)
            .Include(x => x.Post)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (comment is null) {
            return NotFound(id);
        }

        return Ok(comment);
    }
}