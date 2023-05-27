using System.Security.Claims;
using System.Text.Json;
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
    private JsonSerializerOptions _jsonSerializerOptions;

    public CommentsController(ILogger<PostsController> logger, ApplicationContext db) {
        _logger = logger;
        _db = db;
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    [HttpPost("/api/posts/{postId:guid}/comments")]
    public async Task<IActionResult> NewComment([FromRoute] Guid postId, [FromBody] string text) {
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

        var comment = new Comment() {
            Author = user,
            Text = _matchHtmlTags.Replace(text, ""),
            Post = post
        };
        
        await _db.Comments.AddAsync(comment);

        await _db.SaveChangesAsync();
        
        return FixedOk(comment);
    }
    
    private ContentResult FixedOk<T>(T obj)
    {
        return Content(JsonSerializer.Serialize(obj, _jsonSerializerOptions), "application/json");
    }

    [HttpGet("/api/posts/{postId:guid}/comments")]
    public async Task<IActionResult> GetComments([FromRoute] Guid postId) {
        var post = await _db.Posts.FindAsync(postId);
        if (post is null) {
            return NotFound(postId);
        }

        var comments = post.Comments.OrderByDescending(x => x.CreationDate);
        return Ok(comments);
    }

    [HttpPost("{id:guid}/like")]
    public async Task<IActionResult> LikeComment([FromRoute] Guid id) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) {
            return Unauthorized();
        }

        var comment = await _db.Comments.FindAsync(id);
        if (comment is null) {
            return NotFound(id);
        }

        var user = await _db.Users.FindAsync(Guid.Parse(userGuid));
        var isLiked = comment.LikedBy.Contains(user);

        if (isLiked) {
            comment.LikedBy.Remove(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Комментарий {id} удален из понравившихся пользователя {userGuid}");
            return Ok(false);
        }

        comment.LikedBy.Add(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation($"Комментарий {id} добавлен в понравившиеся пользователя {userGuid}");
        return Ok(true);
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
    
    [HttpGet("{id:guid}/like")]
    public async Task<IActionResult> GetLiked([FromRoute] Guid id) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

        var comment = await _db.Comments.FindAsync(id);
        if (comment is null) return NotFound(id);

        var isLiked = comment.LikedBy.Any(x => x.Id == Guid.Parse(userGuid));
        return Ok(isLiked);
    }

}