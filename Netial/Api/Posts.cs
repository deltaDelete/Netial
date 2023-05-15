using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netial.Database;
using Netial.Database.Models;

namespace Netial.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostsController : ControllerBase {
    private readonly ILogger<PostsController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationContext _db;
    private static Regex _matchHtmlTags = new Regex("<[^>]*>");
    private readonly string _webRoot;

    public PostsController(ILogger<PostsController> logger, IWebHostEnvironment environment, ApplicationContext db) {
        _logger = logger;
        _environment = environment;
        _db = db;
        _webRoot = environment.WebRootPath;
    }

    [HttpPost("redirect")]
    public async Task<IActionResult> Redirect([FromForm] string action, [FromForm] Guid id) {
        return RedirectPermanentPreserveMethod($"/api/Posts/{id}/{action}");
    }

    [HttpPost("{id:guid}/upvote")]
    public async Task<IActionResult> UpvotePost([FromRoute] Guid id) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound(id);

        var users = _db.Users
            .Include(u => u.UpvotedPosts);

        var user = users.First(u => u.Id == Guid.Parse(userGuid));

        if (user.UpvotedPosts.Any(x => x == post)) {
            user.UpvotedPosts.Remove(post);
        }
        else {
            user.UpvotedPosts.Add(post);
        }

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("{id:guid}/downvote")]
    public async Task<IActionResult> DownvotePost([FromRoute] Guid id) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound(id);

        var users = _db.Users
            .Include(u => u.DownvotedPosts);
        var user = users.First(u => u.Id == Guid.Parse(userGuid));

        if (user.DownvotedPosts.Any(x => x == post)) {
            user.DownvotedPosts.Remove(post);
        }
        else {
            user.DownvotedPosts.Add(post);
        }

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> NewPostFromForm(
        [FromForm(Name = "author")] Guid authorId,
        [FromForm(Name = "text")] string text, 
        [FromForm] IFormFileCollection? files) {
        var user = await _db.Users.FindAsync(authorId);
        if (user is null) {
            return NotFound(authorId);
        }

        var post = new Post {
            Author = user,
            Text = _matchHtmlTags.Replace(text, "")
        };
        
        if (files is not null) { 
            post.Attachments = await UploadMultipleAttachmentsAsync(files);
        }

        await _db.Posts.AddAsync(post);

        await _db.SaveChangesAsync();

        return Ok();
    }

    [NonAction]
    public async Task<ICollection<Attachment>> UploadMultipleAttachmentsAsync(IEnumerable<IFormFile> files) {
        var attachments = new List<Attachment>();
        foreach (var file in files) {
            attachments.Add(await UploadAttachmentAsync(file));
        }

        return attachments;
    }

    [NonAction]
    public async Task<Attachment> UploadAttachmentAsync(IFormFile file) {
        var guid = Guid.NewGuid();
        using var fs = System.IO.File.Create($"{_webRoot}/images/attachments/{guid}.jpg");
        await file.CopyToAsync(fs);
        var attachment = new Attachment() {
            Id = guid,
            Description = "Картинка"
        };
        return attachment;
    }

    [HttpPost("{id:guid}/view")]
    public async Task<IActionResult> MarkViewedPost([FromRoute] Guid id) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

        var user = await _db.Users.FindAsync(Guid.Parse(userGuid));
        if (user is null) {
            return Unauthorized();
        }

        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound(id);

        post.ViewedBy.Add(user);

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("attachments")]
    public async Task<IActionResult> NewAttachment([FromForm] UploadModel model) {
        var attachment = await UploadAttachmentAsync(model.File);
        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync();
        return Ok(attachment);
    }
    
    public class UploadModel
    {
        //Other inputs
        [Required]
        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }
    }
}