using RBAC.Models;
using RBAC.Utils.Security;

namespace RBAC.Data;

// 使用 List 模拟数据库，重启程序后数据会恢复初始状态。
public class InMemoryDatabase
{
    public List<User> Users { get; } = new List<User>();
    public List<Role> Roles { get; } = new List<Role>();

    public InMemoryDatabase()
    {
        Role adminRole = new Role();
        adminRole.Name = "管理员";
        adminRole.Permissions.Add("user:view");
        adminRole.Permissions.Add("user:create");
        adminRole.Permissions.Add("user:change-role");
        adminRole.Permissions.Add("report:view");

        Role userRole = new Role();
        userRole.Name = "普通用户";
        userRole.Permissions.Add("report:view");

        Roles.Add(adminRole);
        Roles.Add(userRole);

        User admin = new User();
        admin.Username = "admin";
        admin.PasswordHash = PasswordHasher.Hash("Admin123!");
        admin.Role = adminRole;
        Users.Add(admin);

        User alice = new User();
        alice.Username = "alice";
        alice.PasswordHash = PasswordHasher.Hash("User123!");
        alice.Role = userRole;
        Users.Add(alice);
    }
}
