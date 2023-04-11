namespace Netial.Models;

[Flags]
public enum GroupPermissions {
    BanUser,
    BanPost,
    BanMessage,
    RemoveUser
}