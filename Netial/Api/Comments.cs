using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Netial.Database;
using Netial.Database.Models;

namespace Netial.Api; 

public static class Comments {
    private static Regex _matchHtmlTags = new Regex("<[^>]*>");
    public static void ConfigureCommentsApi(this WebApplication app) {
        app.MapPost("/comments/new/{postid}", NewComment);
        app.MapPost("/comments/like/{id}", LikeComment);
    }

    private static async Task<IResult> NewComment(HttpContext context, ApplicationContext db, string postid) {
        var form = context.Request.Form;
        var userGuid = context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Results.Unauthorized();

        if (!Guid.TryParse(userGuid, out Guid authorId)) {
            return Results.Unauthorized();
        }

        if (!Guid.TryParse(postid, out Guid postId)) {
            return Results.BadRequest($"Неверный идентификатор поста {postid}");
        }

        var user = await db.Users.FindAsync(authorId);
        if (user is null) {
            return Results.NotFound($"Не найден пользователь с идентификатором {authorId}");
        }

        var post = await db.Posts.FindAsync(postId);
        if (post is null) {
            return Results.NotFound($"Не найден пост с идентификатором {postId}");
        }

        string text = form["text"];

        await db.Comments.AddAsync(new Comment() {
            Author = user,
            Text = _matchHtmlTags.Replace(text, ""),
            Post = post
        });

        await db.SaveChangesAsync();
        
        return Results.Ok();
    }

    private static async Task<IResult> LikeComment(HttpContext context, ApplicationContext db, string id) {
        var userGuid = context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (string.IsNullOrEmpty(userGuid)) return Results.Unauthorized();

        var comment = await db.Comments.FindAsync(Guid.Parse(id));
        if (comment is null) return Results.NotFound(id);
        
        var users = db.Users
            .Include(u => u.LikedComments);
            
        var user = users.First(u => u.Id == Guid.Parse(userGuid));

        if (user.LikedComments.Any(x => x == comment)) {
            user.LikedComments.Remove(comment);
            await db.SaveChangesAsync();
            return Results.Ok($"Комментарий успешно {id} удален из понравившихся");
        }
        else {
            user.LikedComments.Add(comment);
            await db.SaveChangesAsync();
            return Results.Ok($"Комментарий успешно {id} добавлен в понравившиеся");
        }
    }
}