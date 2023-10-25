using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Netial.Helpers;
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
    private readonly IDbContextFactory<ApplicationContext> _dbContextFactory;

    public PostsController(ILogger<PostsController> logger, IWebHostEnvironment environment, IDbContextFactory<ApplicationContext> dbContextFactory) {
        _logger = logger;
        _environment = environment;
        _dbContextFactory = dbContextFactory;
        _db = dbContextFactory.CreateDbContext();
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

        bool isUpvoted = false;
        if (user.UpvotedPosts.Any(x => x == post)) {
            user.UpvotedPosts.Remove(post);
            isUpvoted = false;
        }
        else {
            user.UpvotedPosts.Add(post);
            isUpvoted = true;
        }

        await _db.SaveChangesAsync();

        return Ok(isUpvoted);
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

        bool isDownvoted;
        if (user.DownvotedPosts.Any(x => x == post)) {
            user.DownvotedPosts.Remove(post);
            isDownvoted = false;
        }
        else {
            user.DownvotedPosts.Add(post);
            isDownvoted = true;
        }

        await _db.SaveChangesAsync();

        return Ok(isDownvoted);
    }

    [HttpGet("{id:guid}/upvote")]
    public async Task<IActionResult> IsUpvoted([FromRoute] Guid id) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound(id);

        var isUpvoted = post.UpvotedBy.Any(x => x.Id == Guid.Parse(userGuid));
        return Ok(isUpvoted);
    }

    [HttpPost("attachments")]
    public async Task<IActionResult> PostAttachments([FromForm] IFormFileCollection files) {
        var attachments = await UploadMultipleAttachmentsAsync(files);
        var db = await _dbContextFactory.CreateDbContextAsync();
        db.Attachments.AddRange(attachments);
        await db.SaveChangesAsync();
        return this.FixedOk(attachments);
    }

    [HttpGet("{id:guid}/downvote")]
    public async Task<IActionResult> IsDownvoted([FromRoute] Guid id) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Unauthorized();

        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound(id);

        var isDownvoted = post.DownvotedBy.Any(x => x.Id == Guid.Parse(userGuid));
        return Ok(isDownvoted);
    }

    [HttpPost]
    public async Task<IActionResult> NewPost([FromBody] PostBody body) {
        var userGuid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var authorId = Guid.Parse(userGuid);
        var db = await _dbContextFactory.CreateDbContextAsync();
        var user = await db.Users.FindAsync(authorId);
        if (user is null) {
            return NotFound(authorId);
        }

        var post = new Post {
            Author = user,
            Text = _matchHtmlTags.Replace(body.Text, "")
        };


        await db.Posts.AddAsync(post);
        await db.SaveChangesAsync();

        if (body.Attachments is not null && body.Attachments.Any()) {
            var attachments = db.Attachments.Where(x => body.Attachments.Contains(x)).Select(x=> x.Id).ToListAsync();
            var postFound = await db.Posts.Include(x => x.Attachments).FirstAsync(x => x.Id == post.Id);
            foreach (var attachment in await attachments) {
                var attachmentFound = await _db.Attachments.FindAsync(attachment);
                if (attachmentFound is not null) {
                    postFound.Attachments.Add(attachmentFound);
                }
            }
        }
        await db.SaveChangesAsync();

        return this.FixedOk(post);
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

    [HttpPost("attachment")]
    public async Task<IActionResult> NewAttachment([FromForm] UploadModel model) {
        var attachment = await UploadAttachmentAsync(model.File);
        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync();
        return this.FixedOk(attachment);
    }

    [HttpGet]
    public async Task<IActionResult> GetPosts([FromQuery] CollectionSettings? settings = null,
        [FromQuery] PostSort? sort = PostSort.New) {
        settings ??= new CollectionSettings() { Take = 10 };
        if (sort == PostSort.Hot) {
            return Ok(await PostsByUpvotes(settings));
        }

        return Ok(await Posts(settings));
    }

    private async Task<IEnumerable<Post>> PostsByUpvotes(CollectionSettings settings) {
        var posts = _db.Posts.Where(x => x.CreationDate.Day > DateTime.Today.Day - 7)
            .OrderByDescending(x => x.Upvotes)
            .Skip(settings.Skip!.Value)
            .Take(settings.Take);
        return await posts.ToListAsync();
    }

    private async Task<IEnumerable<Post>> Posts(CollectionSettings settings) {
        var posts = _db.Posts
            .OrderByDescending(x => x.CreationDate)
            .Skip(settings.Skip!.Value)
            .Take(settings.Take);
        return await posts.ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPost([FromRoute] Guid id) {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) {
            return NotFound(id);
        }

        return this.FixedOk(post);
    }

    [HttpGet("{id:guid}/attachements")]
    public async Task<IActionResult> GetPostAttachments([FromRoute] Guid id) {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) {
            return NotFound(id);
        }

        return Ok(post.Attachments);
    }

    public class UploadModel {
        //Other inputs
        [Required]
        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }
    }

    public enum PostSort {
        New,
        Hot
    }

    public class CollectionSettings {
        public int Take { get; set; } = 10;
        public int? Skip { get; set; } = 0;
    }

    public class PostBody {
        [MinLength(1)]
        public string Text { get; set; }
        public ICollection<Attachment>? Attachments { get; set; }
    }
}