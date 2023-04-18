namespace Netial.Database.Models;

[Flags]
public enum GroupPermissions {
    BanUser = 1,
    BanPost = 2,
    BanMessage = 4,
    RemoveUser = 8
}