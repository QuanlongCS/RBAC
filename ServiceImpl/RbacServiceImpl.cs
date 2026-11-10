using RBAC.Data;
using RBAC.Models;
using RBAC.Service;
using RBAC.Utils.Security;

namespace RBAC.ServiceImpl;

public class RbacServiceImpl : IRbacService
{
    private InMemoryDatabase _database;
    public User? CurrentUser { get; private set; }

    public RbacServiceImpl(InMemoryDatabase database)
    {
        _database = database;
    }

    public bool Login(string username, string password)
    {
        CurrentUser = null;
        foreach (User user in _database.Users)
        {
            if (user.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase)
                && PasswordHasher.Verify(password, user.PasswordHash))
            {
                CurrentUser = user;
                return true;
            }
        }

        return false;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public bool HasPermission(string permission)
    {
        if (CurrentUser == null)
        {
            return false;
        }

        // 用户 → 角色 → 权限：这是本项目最核心的判断。
        foreach (string item in CurrentUser.Role.Permissions)
        {
            if (item == permission)
            {
                return true;
            }
        }

        return false;
    }

    public List<User> GetUsers()
    {
        List<User> result = new List<User>();
        if (HasPermission("user:view"))
        {
            foreach (User user in _database.Users)
            {
                result.Add(user);
            }
        }

        return result;
    }

    public string CreateUser(string username, string password)
    {
        if (!HasPermission("user:create"))
        {
            return "权限不足，不能创建用户。";
        }

        username = username.Trim();
        if (username.Length < 2 || username.Length > 20 || string.IsNullOrWhiteSpace(username))
        {
            return "用户名需要 2～20 个字符，不能全为空白。";
        }

        if (password.Length < 6 || string.IsNullOrWhiteSpace(password))
        {
            return "密码至少 6 位，不能全为空白。";
        }

        foreach (User user in _database.Users)
        {
            if (user.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                return "用户名已存在。";
            }
        }

        User newUser = new User();
        newUser.Username = username;
        newUser.PasswordHash = PasswordHasher.Hash(password);
        newUser.Role = _database.Roles[1]; // 初始数据中的“普通用户”角色。
        _database.Users.Add(newUser);
        return "创建成功，新用户的角色是普通用户。";
    }

    public string ChangeRole(string username, string roleName)
    {
        if (!HasPermission("user:change-role"))
        {
            return "权限不足，不能修改角色。";
        }

        User? targetUser = null;
        foreach (User user in _database.Users)
        {
            if (user.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                targetUser = user;
                break;
            }
        }

        if (targetUser == null)
        {
            return "用户不存在。";
        }

        foreach (Role role in _database.Roles)
        {
            if (role.Name == roleName.Trim())
            {
                // 避免把最后一个管理员改为普通用户，导致无法继续管理。
                int adminCount = 0;
                foreach (User user in _database.Users)
                {
                    if (user.Role.Name == "管理员")
                    {
                        adminCount++;
                    }
                }

                if (targetUser.Role.Name == "管理员" && role.Name != "管理员" && adminCount == 1)
                {
                    return "不能修改最后一个管理员的角色。";
                }

                targetUser.Role = role;
                return "角色修改成功。";
            }
        }

        return "角色不存在，请输入 管理员 或 普通用户。";
    }

    public string ViewReport()
    {
        if (!HasPermission("report:view"))
        {
            return "权限不足，不能查看报表。";
        }

        return "演示报表：订单数 12，销售额 1280 元。";
    }
}
