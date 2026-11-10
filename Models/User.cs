namespace RBAC.Models;

public class User
{
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    // 入门版：一个用户只有一个角色。
    public Role Role { get; set; } = new Role();
}
