using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Netial.Database;
using Netial.Database.Models;

namespace Netial.Api; 

public static class PostsController {
    private static Regex _matchHtmlTags = new Regex("<[^>]*>");
    public static void ConfigurePostsApi(this WebApplication app) {
        app.MapPost("/posts/new", NewPost);
        app.MapPost("/posts/upvote/{id}", UpvotePost);
        app.MapPost("/posts/downvote/{id}", DownvotePost);
        app.MapPost("/posts/view/{id}", MarkViewedPost);
        app.MapPost("/posts", Posts);
    }

    private static async Task<IResult> Posts(HttpContext context, HttpResponse response) {
        var form = context.Request.Form;
        //response.Redirect($"/posts/{form["action"]}/{form["id"]}", true, true);
        return Results.Redirect($"/posts/{form["action"]}/{form["id"]}", false, true);
    }

    private static async Task<IResult> UpvotePost(string id, HttpContext context, ApplicationContext db) {
        var userGuid = context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Results.Unauthorized();

        var post = await db.Posts.FindAsync(Guid.Parse(id));
        if (post is null) return Results.NotFound(id);
        
        var users = db.Users
            .Include(u => u.UpvotedPosts);
            
        var user = users.First(u => u.Id == Guid.Parse(userGuid));

        if (user.UpvotedPosts.Any(x => x == post)) {
            user.UpvotedPosts.Remove(post);
        }
        else {
            user.UpvotedPosts.Add(post);
        }

        await db.SaveChangesAsync();

        return Results.Ok();
    }
    private static async Task<IResult> DownvotePost(string id, HttpContext context, ApplicationContext db) {
        var userGuid = context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Results.Unauthorized();

        var post = await db.Posts.FindAsync(Guid.Parse(id));
        if (post is null) return Results.NotFound(id);

        var users = db.Users
            .Include(u => u.DownvotedPosts);
        var user = users.First(u => u.Id == Guid.Parse(userGuid));

        if (user.DownvotedPosts.Any(x => x == post)) {
            user.DownvotedPosts.Remove(post);
        }
        else {
            user.DownvotedPosts.Add(post);
        }

        await db.SaveChangesAsync();

        return Results.Ok();
    }

    private static async Task<IResult> NewPost(HttpContext context, ApplicationContext db) {
        var form = context.Request.Form;

        if (!Guid.TryParse(form["author"], out Guid authorId)) {
            return Results.BadRequest(form["author"]);
        }

        var user = await db.Users.FindAsync(authorId);
        if (user is null) {
            return Results.NotFound(authorId);
        }
        
        string text = form["text"];

        await db.Posts.AddAsync(new Post() {
            Author = user,
            Text = _matchHtmlTags.Replace(text, "")
        });

        await db.SaveChangesAsync();
        
        return Results.Ok();
    }

    private static async Task<IResult> MarkViewedPost(string id, HttpContext context, ApplicationContext db) {
        var userGuid = context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Results.Unauthorized();

        var user = await db.Users.FindAsync(Guid.Parse(userGuid));
        if (user is null) {
            return Results.Unauthorized();
        }

        if (!Guid.TryParse(id, out Guid postId)) return Results.BadRequest(id);
        var post = await db.Posts.FindAsync(postId);
        if (post is null) return Results.NotFound(id);
        
        post.ViewedBy.Add(user);

        await db.SaveChangesAsync();

        return Results.Ok();
    }
}