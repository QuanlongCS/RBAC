namespace RBAC.Models;

public class Role
{
    public string Name { get; set; } = "";

    // 权限直接用字符串表示，例如 "user:view" 表示可以查看用户。
    public List<string> Permissions { get; set; } = new List<string>();
}
